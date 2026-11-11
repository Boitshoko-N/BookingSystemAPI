using BookingSystemAPI.Models;

namespace BookingSystemAPI.Services
{
    public interface IBookingNotifier
    {
        Task BookingCreatedAsync(Booking booking);

        // previousSlot is the slot the booking was moved away from.
        Task BookingUpdatedAsync(Booking booking, Slot previousSlot);

        Task BookingCancelledAsync(Booking booking, bool cancelledByStaff);
    }

    // Temporary stand-in until the email service is added in Stage 5.
    public class LoggingBookingNotifier : IBookingNotifier
    {
        private readonly ILogger<LoggingBookingNotifier> _logger;

        public LoggingBookingNotifier(ILogger<LoggingBookingNotifier> logger)
        {
            _logger = logger;
        }

        public Task BookingCreatedAsync(Booking booking)
        {
            _logger.LogInformation("Booking {BookingNumber} was created.", booking.BookingNumber);
            return Task.CompletedTask;
        }

        public Task BookingUpdatedAsync(Booking booking, Slot previousSlot)
        {
            _logger.LogInformation("Booking {BookingNumber} was moved from slot {OldSlot} to slot {NewSlot}.",
                booking.BookingNumber, previousSlot.SlotId, booking.SlotId);
            return Task.CompletedTask;
        }

        public Task BookingCancelledAsync(Booking booking, bool cancelledByStaff)
        {
            _logger.LogInformation("Booking {BookingNumber} was cancelled (by staff: {ByStaff}).",
                booking.BookingNumber, cancelledByStaff);
            return Task.CompletedTask;
        }
    }
}