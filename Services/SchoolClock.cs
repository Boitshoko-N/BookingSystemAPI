using BookingSystemAPI.Configuration;
using Microsoft.Extensions.Options;

namespace BookingSystemAPI.Services
{
    public interface ISchoolClock
    {
        // The current date and time in the school's time zone.
        DateTime Now { get; }
    }

    public class SchoolClock : ISchoolClock
    {
        private readonly TimeZoneInfo _timeZone;

        public SchoolClock(IOptions<BookingRulesSettings> options)
        {
            _timeZone = FindTimeZone(options.Value.SchoolTimeZoneId);
        }

        public DateTime Now => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _timeZone);

        private static TimeZoneInfo FindTimeZone(string id)
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(id);
            }
            catch (TimeZoneNotFoundException)
            {
                try
                {
                    return TimeZoneInfo.FindSystemTimeZoneById("South Africa Standard Time");
                }
                catch (TimeZoneNotFoundException)
                {
                    throw new InvalidOperationException(
                        $"The time zone '{id}' was not found. Check BookingRules:SchoolTimeZoneId in appsettings.json.");
                }
            }
        }
    }
}