using System.Security.Claims;

namespace BookingSystemAPI.Helpers
{
    public static class ClaimsExtensions
    {
        // The ParentId (for parents) or StaffNo (for staff) stored in the token.
        public static int GetUserId(this ClaimsPrincipal user)
        {
            var value = user.FindFirstValue("sub");
            if (!int.TryParse(value, out var id))
            {
                throw new UnauthorizedAccessException("Your session is not valid. Please log in again.");
            }
            return id;
        }

        public static string GetRole(this ClaimsPrincipal user)
        {
            return user.FindFirstValue("role") ?? string.Empty;
        }
    }
}