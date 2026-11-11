namespace BookingSystemAPI.Models
{
    public enum SlotStatus
    {
        Open,
        Closed,
        Cancelled
    }

    public enum BookingStatus
    {
        Confirmed,
        Cancelled,
        Completed
    }

    public enum AssessmentOutcome
    {
        Pending,
        Passed,
        Failed
    }

    public enum EmailType
    {
        BookingConfirmation,
        BookingUpdated,
        BookingCancelled,
        AssessmentFeedback,
        AssessmentFeedbackCorrected
    }

    public enum EmailStatus
    {
        Sent,
        Failed
    }
}