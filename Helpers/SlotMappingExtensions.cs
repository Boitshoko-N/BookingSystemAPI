using BookingSystemAPI.DTOs;
using BookingSystemAPI.Models;

namespace BookingSystemAPI.Helpers
{
    public static class SlotMappingExtensions
    {
        public static DateTime StartsAt(this Slot slot) => slot.AssessmentDate.ToDateTime(slot.StartTime);

        // The three rules: Open, not full, and not already started.
        // activeBookings = bookings that are not Cancelled. schoolNow = current school time.
        public static bool IsBookable(this Slot slot, int activeBookings, DateTime schoolNow)
        {
            return slot.Status == SlotStatus.Open
                   && activeBookings < slot.MaxBookings
                   && slot.StartsAt() > schoolNow;
        }

        public static SlotResponseDto ToDto(this Slot slot, int activeBookings, DateTime schoolNow)
        {
            bool isFull = activeBookings >= slot.MaxBookings;

            return new SlotResponseDto
            {
                SlotId = slot.SlotId,
                AssessmentDate = slot.AssessmentDate,
                StartTime = slot.StartTime,
                EndTime = slot.EndTime,
                MaxBookings = slot.MaxBookings,
                ActiveBookings = activeBookings,
                AvailablePlaces = Math.Max(0, slot.MaxBookings - activeBookings),
                Status = slot.Status,
                IsFull = isFull,
                IsBookable = slot.IsBookable(activeBookings, schoolNow),
                DisplayStatus = slot.Status == SlotStatus.Open && isFull ? "Full" : slot.Status.ToString()
            };
        }
    }
}