using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace BookingSystemAPI.Helpers
{
    public static class DbUpdateExceptionExtensions
    {
        public static bool IsUniqueConstraintViolation(this DbUpdateException exception)
        {
            return exception.InnerException is SqlException sql &&
                   (sql.Number == 2601 || sql.Number == 2627);
        }
    }
}