namespace BookingSystemAPI.Models
{
    public static class RoleNames
    {
        public const string Admin = "Admin";
        public const string Clerk = "Clerk";
        public const string Parent = "Parent";

        // Used in [Authorize(Roles = RoleNames.Staff)] to allow Admin or Clerk.
        public const string Staff = Admin + "," + Clerk;
    }
}