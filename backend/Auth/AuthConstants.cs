namespace AdvancedOrderSystem.Auth;

public static class AuthConstants
{
    public const string AdminRole = "Admin";
    public const string AdminPolicy = "AdminOnly";
    public const string CookieName = "aos.auth";

    public const string AuthRateLimitPolicy = "auth";

    public static readonly TimeSpan RememberMeDuration = TimeSpan.FromDays(7);

    // Sliding lifetime of a normal (not remembered) session
    public static readonly TimeSpan SessionDuration = TimeSpan.FromHours(8);
}