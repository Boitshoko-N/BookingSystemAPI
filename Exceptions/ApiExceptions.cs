namespace BookingSystemAPI.Exceptions
{
    // 400: the request is understood but breaks a business rule.
    public class BusinessRuleException : Exception
    {
        public BusinessRuleException(string message) : base(message) { }
    }

    // 403: the user is logged in but is not allowed to do this.
    public class ForbiddenException : Exception
    {
        public ForbiddenException(string message) : base(message) { }
    }

    // 404: the record does not exist (or is not visible to this user).
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) { }
    }

    // 409: the request clashes with the current state, e.g. slot full, duplicate email.
    public class ConflictException : Exception
    {
        public ConflictException(string message) : base(message) { }
    }
}