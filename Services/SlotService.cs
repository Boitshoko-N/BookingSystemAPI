using BookingSystemAPI.Data;
using BookingSystemAPI.DTOs;
using BookingSystemAPI.Exceptions;
using BookingSystemAPI.Helpers;
using BookingSystemAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingSystemAPI.Services
{
    public interface ISlotService
    {
        Task<List<SlotResponseDto>> GetAllAsync(DateOnly? from, DateOnly? to, SlotStatus? status);
        Task<List<SlotResponseDto>> GetAvailableAsync();
        Task<SlotResponseDto> GetByIdAsync(int slotId);
        Task<SlotResponseDto> CreateAsync(CreateSlotDto dto);
        Task<SlotResponseDto> UpdateAsync(int slotId, UpdateSlotDto dto);
        Task DeleteAsync(int slotId);
    }

    public class SlotService : ISlotService
    {
        private readonly ApplicationDbContext _context;
        private readonly ISchoolClock _clock;

        public SlotService(ApplicationDbContext context, ISchoolClock clock)
        {
            _context = context;
            _clock = clock;
        }

        // ---------- Reading ----------

        public async Task<List<SlotResponseDto>> GetAllAsync(DateOnly? from, DateOnly? to, SlotStatus? status)
        {
            var query = _context.Slots.AsNoTracking().AsQueryable();

            if (from.HasValue) query = query.Where(s => s.AssessmentDate >= from.Value);
            if (to.HasValue) query = query.Where(s => s.AssessmentDate <= to.Value);
            if (status.HasValue) query = query.Where(s => s.Status == status.Value);

            var rows = await query
                .OrderBy(s => s.AssessmentDate)
                .ThenBy(s => s.StartTime)
                .Select(s => new
                {
                    Slot = s,
                    Active = s.Bookings.Count(b => b.Status != BookingStatus.Cancelled)
                })
                .ToListAsync();

            var now = _clock.Now;
            return rows.Select(r => r.Slot.ToDto(r.Active, now)).ToList();
        }

        // Only slots a parent can actually book right now.
        public async Task<List<SlotResponseDto>> GetAvailableAsync()
        {
            var now = _clock.Now;
            var today = DateOnly.FromDateTime(now);

            var rows = await _context.Slots
                .AsNoTracking()
                .Where(s => s.Status == SlotStatus.Open && s.AssessmentDate >= today)
                .OrderBy(s => s.AssessmentDate)
                .ThenBy(s => s.StartTime)
                .Select(s => new
                {
                    Slot = s,
                    Active = s.Bookings.Count(b => b.Status != BookingStatus.Cancelled)
                })
                .ToListAsync();

            // The same rule that booking creation uses, applied to the loaded rows.
            return rows
                .Where(r => r.Slot.IsBookable(r.Active, now))
                .Select(r => r.Slot.ToDto(r.Active, now))
                .ToList();
        }

        public async Task<SlotResponseDto> GetByIdAsync(int slotId)
        {
            var row = await _context.Slots
                .AsNoTracking()
                .Where(s => s.SlotId == slotId)
                .Select(s => new
                {
                    Slot = s,
                    Active = s.Bookings.Count(b => b.Status != BookingStatus.Cancelled)
                })
                .FirstOrDefaultAsync();

            if (row == null)
            {
                throw new NotFoundException("Slot not found.");
            }

            return row.Slot.ToDto(row.Active, _clock.Now);
        }

        // ---------- Creating ----------

        public async Task<SlotResponseDto> CreateAsync(CreateSlotDto dto)
        {
            ValidateTimes(dto.StartTime, dto.EndTime);
            EnsureStartsInFuture(dto.AssessmentDate, dto.StartTime);

            var slot = new Slot
            {
                AssessmentDate = dto.AssessmentDate,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                MaxBookings = dto.MaxBookings,
                Status = SlotStatus.Open
            };

            _context.Slots.Add(slot);
            await _context.SaveChangesAsync();

            return slot.ToDto(0, _clock.Now);
        }

        // ---------- Updating ----------

        public async Task<SlotResponseDto> UpdateAsync(int slotId, UpdateSlotDto dto)
        {
            ValidateTimes(dto.StartTime, dto.EndTime);

            // Lock the slot so nobody can book it while we check and change it.
            await using var transaction = await _context.Database.BeginTransactionAsync();

            var slot = await _context.LockSlotAsync(slotId);
            if (slot == null)
            {
                throw new NotFoundException("Slot not found.");
            }

            int active = await CountActiveBookingsAsync(slotId);

            bool timeChanged = slot.AssessmentDate != dto.AssessmentDate
                               || slot.StartTime != dto.StartTime
                               || slot.EndTime != dto.EndTime;

            if (timeChanged)
            {
                if (active > 0)
                {
                    throw new ConflictException(
                        $"This slot has {BookingsText(active)}, so its date and times cannot be changed. Cancel or move the bookings first.");
                }

                EnsureStartsInFuture(dto.AssessmentDate, dto.StartTime);
            }

            if (dto.MaxBookings < active)
            {
                throw new ConflictException(
                    $"This slot already has {BookingsText(active)}, so the maximum cannot be lower than {active}.");
            }

            if (active > 0 && dto.Status != SlotStatus.Open)
            {
                throw new ConflictException(
                    $"This slot has {BookingsText(active)} and cannot be set to {dto.Status}. Cancel or move the bookings first.");
            }

            slot.AssessmentDate = dto.AssessmentDate;
            slot.StartTime = dto.StartTime;
            slot.EndTime = dto.EndTime;
            slot.MaxBookings = dto.MaxBookings;
            slot.Status = dto.Status;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return slot.ToDto(active, _clock.Now);
        }

        // ---------- Deleting ----------

        // Only a slot that has NEVER had a booking (not even a cancelled one) can be deleted.
        public async Task DeleteAsync(int slotId)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            var slot = await _context.LockSlotAsync(slotId);
            if (slot == null)
            {
                throw new NotFoundException("Slot not found.");
            }

            int active = await CountActiveBookingsAsync(slotId);
            if (active > 0)
            {
                throw new ConflictException(
                    $"This slot has {BookingsText(active)} and cannot be deleted. Cancel or move the bookings first, then close the slot.");
            }

            bool hasHistory = await _context.Bookings.AnyAsync(b => b.SlotId == slotId);
            if (hasHistory)
            {
                throw new ConflictException(
                    "This slot has booking history and cannot be deleted. Set its status to Closed or Cancelled instead.");
            }

            _context.Slots.Remove(slot);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }

        // ---------- Private helpers ----------

        private Task<int> CountActiveBookingsAsync(int slotId)
        {
            return _context.Bookings.CountAsync(b => b.SlotId == slotId && b.Status != BookingStatus.Cancelled);
        }

        private static void ValidateTimes(TimeOnly start, TimeOnly end)
        {
            if (end <= start)
            {
                throw new BusinessRuleException("The end time must be after the start time.");
            }
        }

        private void EnsureStartsInFuture(DateOnly date, TimeOnly start)
        {
            if (date.ToDateTime(start) <= _clock.Now)
            {
                throw new BusinessRuleException("The slot must start in the future.");
            }
        }

        private static string BookingsText(int count)
        {
            return count == 1 ? "1 active booking" : $"{count} active bookings";
        }
    }
}