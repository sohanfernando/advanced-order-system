using System.Security.Claims;
using AdvancedOrderSystem.Auth;
using AdvancedOrderSystem.Models.DTOs.Auth;
using AdvancedOrderSystem.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AdvancedOrderSystem.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    [EnableRateLimiting(AuthConstants.AuthRateLimitPolicy)]
    public async Task<ActionResult<AuthUserResponse>> Register(RegisterRequest request)
    {
        var user = await _authService.RegisterAsync(request);

        // The user signs in separately, which lets them choose "remember me"
        return CreatedAtAction(nameof(Me), user);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting(AuthConstants.AuthRateLimitPolicy)]
    public async Task<ActionResult<AuthUserResponse>> Login(LoginRequest request)
    {
        var user = await _authService.ValidateCredentialsAsync(request);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role)
        };

        var principal = new ClaimsPrincipal(
            new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme));

        var now = DateTimeOffset.UtcNow;

        var properties = new AuthenticationProperties
        {
            // Persistent cookie survives closing the browser
            IsPersistent = request.RememberMe,
            IssuedUtc = now,
            ExpiresUtc = now.Add(request.RememberMe
                ? AuthConstants.RememberMeDuration
                : AuthConstants.SessionDuration),

            // Remember me lasts exactly 7 days; normal sessions slide while active
            AllowRefresh = !request.RememberMe
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            properties);

        return Ok(user);
    }

    // Anonymous so an already-expired session can still be cleared
    [HttpPost("logout")]
    [AllowAnonymous]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);

        return NoContent();
    }

    // The frontend calls this on start-up to restore the session
    [HttpGet("me")]
    [Authorize(Policy = AuthConstants.AdminPolicy)]
    public async Task<ActionResult<AuthUserResponse>> Me()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out var userId))
        {
            return Unauthorized();
        }

        var user = await _authService.GetUserAsync(userId);

        // The account was removed after the cookie was issued
        if (user == null)
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return Unauthorized();
        }

        return Ok(user);
    }
}