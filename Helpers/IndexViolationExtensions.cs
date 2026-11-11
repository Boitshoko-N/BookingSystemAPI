using Microsoft.EntityFrameworkCore;

namespace BookingSystemAPI.Helpers
{
    public static class IndexViolationExtensions
    {
        // True if the save failed because this specific unique index was violated.
        public static bool IsViolationOfIndex(this DbUpdateException exception, string indexName)
        {
            return exception.IsUniqueConstraintViolation() &&
                   exception.InnerException != null &&
                   exception.InnerException.Message.Contains(indexName, StringComparison.OrdinalIgnoreCase);
        }
    }
}