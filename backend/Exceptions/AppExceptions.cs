namespace AdvancedOrderSystem.Exceptions;

// 401 - wrong credentials or locked account
public class AuthenticationFailedException : Exception
{
    public AuthenticationFailedException(string message) : base(message)
    {
    }
}

// 403 - authenticated or not, the caller may not do this
public class ForbiddenException : Exception
{
    public ForbiddenException(string message) : base(message)
    {
    }
}

// 409 - the resource already exists
public class ConflictException : Exception
{
    public ConflictException(string message) : base(message)
    {
    }
}
