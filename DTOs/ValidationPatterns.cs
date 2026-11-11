namespace BookingSystemAPI.DTOs
{
    public static class ValidationPatterns
    {
        // 8-100 characters, at least one letter and one digit.
        public const string Password = @"^(?=.*[A-Za-z])(?=.*\d).{8,100}$";
        public const string PasswordMessage = "Password must be 8 to 100 characters and include at least one letter and one digit.";

        // Optional leading +, then digits, spaces or hyphens (e.g. 082 123 4567 or +27821234567).
        public const string Phone = @"^\+?[0-9\s\-]{7,20}$";
        public const string PhoneMessage = "Phone number may only contain digits, spaces, hyphens and an optional leading +.";
    }
}