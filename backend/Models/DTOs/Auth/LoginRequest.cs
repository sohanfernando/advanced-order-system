// LoginRequest.cs
namespace AdvancedOrderSystem.Models.DTOs.Auth;

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    // true = stay signed in for 7 days
    public bool RememberMe { get; set; }
}
