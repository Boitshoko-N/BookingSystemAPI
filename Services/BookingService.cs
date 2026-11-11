using BookingSystemAPI.Configuration;
using BookingSystemAPI.Data;
using BookingSystemAPI.DTOs;
using BookingSystemAPI.Exceptions;
using BookingSystemAPI.Helpers;
using BookingSystemAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BookingSystemAPI.Services
{
    public interface IBookingService
    {
        Task<BookingDetailsDto> CreateAsync(int parentId, CreateBookingDto dto);
        Task<List<BookingResponseDto>> GetMineAsync(int parentId);
        Task<List<BookingResponseDto>> GetAllAsync(int? slotId, BookingStatus? status, DateOnly? from, DateOnly? to, int? studentNo);
        Task<BookingDetailsDto> GetByIdAsync(int bookingId, int userId, string role);
        Task<BookingDetailsDto> GetByNumberAsync(string bookingNumber, int userId, string role);
        Task<BookingDetailsDto> UpdateAsync(int bookingId, UpdateBookingDto dto, int userId, string role);
        Task<BookingDetailsDto> CancelAsync(int bookingId, CancelBookingDto? dto, int userId, string role);
        Task<BookingDetailsDto> CompleteAsync(int bookingId);
        Task<BookingDetailsDto> RevertCompletionAsync(int bookingId, int staffNo);
    }

    public class BookingService : IBookingService
    {
        private const int MaxBookingAttempts = 5;
        private const string BookingNumberIndex = "IX_Bookings_BookingNumber";
        private const string StudentConfirmedIndex = "IX_Bookings_StudentNo";

        private readonly ApplicationDbContext _context;
        private readonly ISchoolClock _clock;
        private readonly IBookingNumberGenerator _numberGenerator;
        private readonly IBookingNotifier _notifier;
        private readonly BookingRulesSettings _rules;
        private readonly ILogger<BookingService> _logger;

        public BookingService(
            ApplicationDbContext context,
            ISchoolClock clock,
            IBookingNumberGenerator numberGenerator,
            IBookingNotifier notifier,
            IOptions<BookingRulesSettings> rules,
            ILogger<BookingService> logger)
        {
            _context = context;
            _clock = clock;
            _numberGenerator = numberGenerator;
            _notifier = notifier;
            _rules = rules.Value;
            _logger = logger;
        }

        // =====================================================================
        // Create
        // =====================================================================

        public async Task<BookingDetailsDto> CreateAsync(int parentId, CreateBookingDto dto)
        {
            // Each attempt is a complete transaction. A booking number clash rolls the
            // attempt back and tries again with a new number.
            for (int attempt = 1; attempt <= MaxBookingAttempts; attempt++)
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();

                // The student must belong to the logged-in parent.
                var student = await _context.Students
                    .Include(s => s.Parent)
                    .FirstOrDefaultAsync(s => s.StudentNo == dto.StudentNo && s.ParentId == parentId);

                if (student == null)
                {
                    throw new NotFoundException("Student not found.");
                }

                // One attempt only: a student who has already completed the assessment cannot book again.
                bool alreadyWrote = await _context.Bookings.AnyAsync(b =>
                    b.StudentNo == student.StudentNo && b.Status == BookingStatus.Completed);

                if (alreadyWrote)
                {
                    throw new ConflictException(
                        $"{student.FirstName} has already completed the intake assessment and cannot make another booking.");
                }

                var existingNumber = await _context.Bookings
                    .Where(b => b.StudentNo == student.StudentNo && b.Status == BookingStatus.Confirmed)
                    .Select(b => b.BookingNumber)
                    .FirstOrDefaultAsync();

                if (existingNumber != null)
                {
                    throw new ConflictException(
                        $"{student.FirstName} already has a confirmed booking ({existingNumber}). Cancel or reschedule it instead.");
                }

                // Lock the slot row. From here until commit, nobody else can book this slot.
                var slot = await _context.LockSlotAsync(dto.SlotId);
                if (slot == null)
                {
                    throw new NotFoundException("Slot not found.");
                }

                int active = await CountActiveBookingsAsync(slot.SlotId);
                EnsureSlotBookable(slot, active);

                var booking = new Booking
                {
                    BookingNumber = await GenerateUniqueBookingNumberAsync(),
                    ParentId = parentId,
                    Parent = student.Parent,
                    StudentNo = student.StudentNo,
                    Student = student,
                    SlotId = slot.SlotId,
                    Slot = slot,
                    Status = BookingStatus.Confirmed,
                    DateMade = DateTime.UtcNow
                };

                _context.Bookings.Add(booking);

                try
                {
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (DbUpdateException ex) when (ex.IsViolationOfIndex(BookingNumberIndex))
                {
                    // Extremely rare: another request picked the same number at the same instant.
                    _logger.LogWarning("Booking number clash on attempt {Attempt}. Retrying with a new number.", attempt);
                    _context.ChangeTracker.Clear();
                    continue;
                }
                catch (DbUpdateException ex) when (ex.IsViolationOfIndex(StudentConfirmedIndex))
                {
                    // The student was booked by another request (e.g. a double-click) a moment ago.
                    throw new ConflictException(
                        $"{student.FirstName} already has a confirmed booking. Cancel or reschedule it instead.");
                }

                await NotifySafelyAsync(() => _notifier.BookingCreatedAsync(booking));
                return booking.ToDetailsDto();
            }

            throw new InvalidOperationException("A unique booking number could not be generated. Please try again.");
        }

        // =====================================================================
        // Reading
        // =====================================================================

        public async Task<List<BookingResponseDto>> GetMineAsync(int parentId)
        {
            var bookings = await _context.Bookings
                .AsNoTracking()
                .Include(b => b.Student)
                .Include(b => b.Slot)
                .Where(b => b.ParentId == parentId)
                .OrderByDescending(b => b.DateMade)
                .ToListAsync();

            return bookings.Select(b => b.ToDto()).ToList();
        }

        // Staff: filter by slot, status, student or slot date range.
        public async Task<List<BookingResponseDto>> GetAllAsync(
            int? slotId, BookingStatus? status, DateOnly? from, DateOnly? to, int? studentNo)
        {
            var query = _context.Bookings
                .AsNoTracking()
                .Include(b => b.Student)
                .Include(b => b.Slot)
                .AsQueryable();

            if (slotId.HasValue) query = query.Where(b => b.SlotId == slotId.Value);
            if (status.HasValue) query = query.Where(b => b.Status == status.Value);
            if (studentNo.HasValue) query = query.Where(b => b.StudentNo == studentNo.Value);
            if (from.HasValue) query = query.Where(b => b.Slot!.AssessmentDate >= from.Value);
            if (to.HasValue) query = query.Where(b => b.Slot!.AssessmentDate <= to.Value);

            var bookings = await query
                .OrderByDescending(b => b.DateMade)
                .ToListAsync();

            return bookings.Select(b => b.ToDto()).ToList();
        }

        public async Task<BookingDetailsDto> GetByIdAsync(int bookingId, int userId, string role)
        {
            var booking = await DetailsQuery()
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            return EnsureVisible(booking, userId, role).ToDetailsDto();
        }

        public async Task<BookingDetailsDto> GetByNumberAsync(string bookingNumber, int userId, string role)
        {
            var number = bookingNumber.Trim().ToUpperInvariant();

            var booking = await DetailsQuery()
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.BookingNumber == number);

            return EnsureVisible(booking, userId, role).ToDetailsDto();
        }

        // =====================================================================
        // Update (reschedule to a different slot)
        // =====================================================================

        public async Task<BookingDetailsDto> UpdateAsync(int bookingId, UpdateBookingDto dto, int userId, string role)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            // Lock the NEW slot first, exactly like when creating a booking.
            var newSlot = await _context.LockSlotAsync(dto.SlotId);
            if (newSlot == null)
            {
                throw new NotFoundException("Slot not found.");
            }

            var booking = EnsureVisible(
                await DetailsQuery().FirstOrDefaultAsync(b => b.BookingId == bookingId), userId, role);

            EnsureCanBeChanged(booking, "changed");

            var previousSlot = booking.Slot!;
            EnsureParentMayChange(previousSlot, role, "changed");

            if (previousSlot.SlotId == newSlot.SlotId)
            {
                throw new BusinessRuleException("The booking is already in this slot.");
            }

            int active = await CountActiveBookingsAsync(newSlot.SlotId);
            EnsureSlotBookable(newSlot, active);

            booking.Slot = newSlot;
            booking.SlotId = newSlot.SlotId;
            booking.LastModifiedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            await NotifySafelyAsync(() => _notifier.BookingUpdatedAsync(booking, previousSlot));
            return booking.ToDetailsDto();
        }

        // =====================================================================
        // Cancel
        // =====================================================================

        // The booking stays in the database with status Cancelled. Because capacity is
        // calculated from non-cancelled bookings, the place is released automatically.
        public async Task<BookingDetailsDto> CancelAsync(int bookingId, CancelBookingDto? dto, int userId, string role)
        {
            var booking = EnsureVisible(
                await DetailsQuery().FirstOrDefaultAsync(b => b.BookingId == bookingId), userId, role);

            EnsureCanBeChanged(booking, "cancelled");

            EnsureParentMayChange(booking.Slot!, role, "cancelled");

            var reason = dto?.Reason?.Trim();

            booking.Status = BookingStatus.Cancelled;
            booking.CancelledAt = DateTime.UtcNow;
            booking.LastModifiedAt = booking.CancelledAt;
            booking.CancellationReason = string.IsNullOrEmpty(reason) ? null : reason;

            await _context.SaveChangesAsync();

            bool cancelledByStaff = role != RoleNames.Parent;
            await NotifySafelyAsync(() => _notifier.BookingCancelledAsync(booking, cancelledByStaff));

            return booking.ToDetailsDto();
        }

        // =====================================================================
        // Complete (the clerk checks the child in at the school)
        // =====================================================================

        // The clerk has looked up the booking number, confirmed the details are correct,
        // and the child is at the school. This does NOT involve marks.
        public async Task<BookingDetailsDto> CompleteAsync(int bookingId)
        {
            var booking = await DetailsQuery().FirstOrDefaultAsync(b => b.BookingId == bookingId);
            if (booking == null)
            {
                throw new NotFoundException("Booking not found.");
            }

            if (booking.Status == BookingStatus.Cancelled)
            {
                throw new ConflictException("This booking has been cancelled and cannot be marked as completed.");
            }

            if (booking.Status == BookingStatus.Completed)
            {
                throw new ConflictException("This booking has already been marked as completed.");
            }

            // A booking for a future date cannot be completed yet.
            var today = DateOnly.FromDateTime(_clock.Now);
            if (booking.Slot!.AssessmentDate > today)
            {
                throw new ConflictException(
                    $"This booking is for {booking.Slot.AssessmentDate:yyyy-MM-dd}. It cannot be marked as completed before the assessment date.");
            }

            booking.Status = BookingStatus.Completed;
            booking.LastModifiedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return booking.ToDetailsDto();
        }

        // =====================================================================
        // Revert completion (Admin only: undo a clerk's mistake)
        // =====================================================================

        // Puts a Completed booking back to Confirmed. Only allowed while no marks exist for the student.
        // Completed and Confirmed both count towards slot capacity, so no capacity check is needed.
        public async Task<BookingDetailsDto> RevertCompletionAsync(int bookingId, int staffNo)
        {
            var booking = await DetailsQuery().FirstOrDefaultAsync(b => b.BookingId == bookingId);
            if (booking == null)
            {
                throw new NotFoundException("Booking not found.");
            }

            if (booking.Status != BookingStatus.Completed)
            {
                throw new ConflictException("Only a completed booking can be reverted.");
            }

            bool marksExist = await _context.AssessmentResults.AnyAsync(r => r.StudentNo == booking.StudentNo);
            if (marksExist)
            {
                throw new ConflictException(
                    "Marks have already been captured for this student, so the completion can no longer be reverted.");
            }

            booking.Status = BookingStatus.Confirmed;
            booking.LastModifiedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.IsViolationOfIndex(StudentConfirmedIndex))
            {
                // Safety net only: the student somehow has another Confirmed booking.
                throw new ConflictException("This student already has another confirmed booking, so this one cannot be reverted.");
            }

            // Recorded in the log so there is a trail of who undid a completion.
            _logger.LogWarning("Completion of booking {BookingNumber} was reverted by staff member {StaffNo}.",
                booking.BookingNumber, staffNo);

            return booking.ToDetailsDto();
        }

        // =====================================================================
        // Private helpers
        // =====================================================================

        // Everything the details DTO and the emails need.
        private IQueryable<Booking> DetailsQuery()
        {
            return _context.Bookings
                .Include(b => b.Parent)
                .Include(b => b.Student)
                .Include(b => b.Slot);
                
        }

        // A parent can only see their own bookings. Anything else looks like "not found".
        private static Booking EnsureVisible(Booking? booking, int userId, string role)
        {
            if (booking == null || (role == RoleNames.Parent && booking.ParentId != userId))
            {
                throw new NotFoundException("Booking not found.");
            }
            return booking;
        }

        private static void EnsureCanBeChanged(Booking booking, string action)
        {
            if (booking.Status == BookingStatus.Cancelled)
            {
                throw new ConflictException(
                    $"This booking has been cancelled and cannot be {action}. Please make a new booking.");
            }

            if (booking.Status == BookingStatus.Completed)
            {
                throw new ConflictException($"This booking is completed and cannot be {action}.");
            }
        }

        // Parents cannot change or cancel close to the assessment. Staff have no cut-off.
        private void EnsureParentMayChange(Slot currentSlot, string role, string action)
        {
            if (role != RoleNames.Parent)
            {
                return;
            }

            var cutoff = currentSlot.StartsAt().AddHours(-_rules.ParentChangeCutoffHours);
            if (_clock.Now >= cutoff)
            {
                throw new ConflictException(
                    $"A booking can only be {action} up to {_rules.ParentChangeCutoffHours} hours before the assessment. Please contact the school.");
            }
        }

        private Task<int> CountActiveBookingsAsync(int slotId)
        {
            return _context.Bookings.CountAsync(b => b.SlotId == slotId && b.Status != BookingStatus.Cancelled);
        }

        // Uses the shared IsBookable rule, then works out WHY it failed to give a clear message.
        private void EnsureSlotBookable(Slot slot, int activeBookings)
        {
            var now = _clock.Now;
            if (slot.IsBookable(activeBookings, now))
            {
                return;
            }

            if (slot.Status != SlotStatus.Open)
            {
                throw new ConflictException("This slot is not open for booking.");
            }

            if (slot.StartsAt() <= now)
            {
                throw new ConflictException("This slot has already started or passed.");
            }

            throw new ConflictException("This slot is full. Please choose another slot.");
        }

        // Layer 1 and 2 of the booking number guarantee: random, then checked against the database.
        // Layer 3 is the unique index, handled in CreateAsync.
        private async Task<string> GenerateUniqueBookingNumberAsync()
        {
            for (int i = 0; i < 10; i++)
            {
                var candidate = _numberGenerator.Generate();
                bool taken = await _context.Bookings.AnyAsync(b => b.BookingNumber == candidate);
                if (!taken)
                {
                    return candidate;
                }
            }

            throw new InvalidOperationException("A unique booking number could not be generated.");
        }

        // The booking is already saved. A notification problem must never undo or hide it.
        private async Task NotifySafelyAsync(Func<Task> notify)
        {
            try
            {
                await notify();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "A booking notification failed. The booking itself was saved.");
            }
        }
    }
}