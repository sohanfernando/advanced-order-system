// AuthUserResponse.cs
namespace AdvancedOrderSystem.Models.DTOs.Auth;

public class AuthUserResponse
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;
}
