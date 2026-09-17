// Services/IAuthService.cs
using AdvancedOrderSystem.Models.DTOs.Auth;

namespace AdvancedOrderSystem.Services;

public interface IAuthService
{
    Task<AuthUserResponse> RegisterAsync(RegisterRequest request);

    // Throws AuthenticationFailedException when the credentials are wrong or the account is locked
    Task<AuthUserResponse> ValidateCredentialsAsync(LoginRequest request);

    Task<AuthUserResponse?> GetUserAsync(int id);
}
