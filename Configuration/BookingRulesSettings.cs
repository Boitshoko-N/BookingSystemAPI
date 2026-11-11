namespace BookingSystemAPI.Configuration
{
    public class BookingRulesSettings
    {
        public const string SectionName = "BookingRules";

        // Parents cannot change or cancel a booking within this many hours of the slot.
        public int ParentChangeCutoffHours { get; set; } = 24;

        // Used to decide whether a slot is "in the past".
        public string SchoolTimeZoneId { get; set; } = "Africa/Johannesburg";
    }
}