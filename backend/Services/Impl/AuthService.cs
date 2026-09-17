// Services/Impl/AuthService.cs
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using AdvancedOrderSystem.Auth;
using AdvancedOrderSystem.Data;
using AdvancedOrderSystem.Exceptions;
using AdvancedOrderSystem.Models.DTOs.Auth;
using AdvancedOrderSystem.Models.Entities;
using AdvancedOrderSystem.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AdvancedOrderSystem.Services.Impl;

public class AuthService : IAuthService
{
    // Must match the column lengths in ApplicationDbContext
    private const int NameMaxLength = 150;
    private const int EmailMaxLength = 200;

    private const int PasswordMinLength = 8;
    private const int PasswordMaxLength = 128;

    private const int UniqueIndexViolation = 2601;
    private const int UniqueConstraintViolation = 2627;

    private const string InvalidCredentialsMessage = "Invalid email or password.";
    private const string DuplicateEmailMessage = "An account with this email already exists.";

    // Checked when the email is unknown, so failed logins take the same time
    // whether or not the account exists
    private static readonly string DummyPasswordHash =
        new PasswordHasher<AdminUser>().HashPassword(new AdminUser(), "not-a-real-password");

    private readonly IAdminUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher<AdminUser> _passwordHasher;
    private readonly AuthOptions _options;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IAdminUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IPasswordHasher<AdminUser> passwordHasher,
        IOptions<AuthOptions> options,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _options = options.Value;
        _logger = logger;
    }

    // ========================================
    // Sign up
    // ========================================

    public async Task<AuthUserResponse> RegisterAsync(RegisterRequest request)
    {
        if (!IsRegistrationKeyValid(request.RegistrationKey))
        {
            throw new ForbiddenException(
                "Invalid registration key.");
        }

        var fullName = request.FullName?.Trim() ?? string.Empty;

        if (fullName.Length == 0)
        {
            throw new ArgumentException(
                "Full name is required.");
        }

        if (fullName.Length > NameMaxLength)
        {
            throw new ArgumentException(
                $"Full name cannot be longer than {NameMaxLength} characters.");
        }

        var email = NormalizeEmail(request.Email);

        if (!IsValidEmail(email))
        {
            throw new ArgumentException(
                "A valid email address is required.");
        }

        ValidatePassword(request.Password);

        if (request.Password != request.ConfirmPassword)
        {
            throw new ArgumentException(
                "Passwords do not match.");
        }

        if (await _userRepository.EmailExistsAsync(email))
        {
            throw new ConflictException(DuplicateEmailMessage);
        }

        var user = new AdminUser
        {
            FullName = fullName,
            Email = email,
            CreatedAt = DateTime.UtcNow
        };

        user.PasswordHash =
            _passwordHasher.HashPassword(user, request.Password);

        await _userRepository.AddAsync(user);

        try
        {
            await _unitOfWork.SaveChangesAsync();
        }
        // Another request registered the same email after the check above
        catch (DbUpdateException ex) when (
            ex.InnerException is SqlException
            {
                Number: UniqueIndexViolation or UniqueConstraintViolation
            })
        {
            throw new ConflictException(DuplicateEmailMessage);
        }

        _logger.LogInformation(
            "Admin account {UserId} registered", user.Id);

        return MapToResponse(user);
    }

    // ========================================
    // Sign in
    // ========================================

    public async Task<AuthUserResponse> ValidateCredentialsAsync(LoginRequest request)
    {
        var email = NormalizeEmail(request.Email);

        if (email.Length == 0 ||
            string.IsNullOrEmpty(request.Password))
        {
            throw new ArgumentException(
                "Email and password are required.");
        }

        var user = await _userRepository.GetByEmailAsync(email);

        if (user == null)
        {
            _passwordHasher.VerifyHashedPassword(
                new AdminUser(), DummyPasswordHash, request.Password);

            throw new AuthenticationFailedException(InvalidCredentialsMessage);
        }

        var now = DateTime.UtcNow;

        if (user.LockoutEndUtc > now)
        {
            throw new AuthenticationFailedException(
                "This account is temporarily locked because of too many failed sign-in attempts. Please try again later.");
        }

        var result = _passwordHasher.VerifyHashedPassword(
            user, user.PasswordHash, request.Password);

        if (result == PasswordVerificationResult.Failed)
        {
            await RegisterFailedAttemptAsync(user, now);

            throw new AuthenticationFailedException(InvalidCredentialsMessage);
        }

        // Upgrade hashes created with older hashing settings
        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash =
                _passwordHasher.HashPassword(user, request.Password);
        }

        user.FailedLoginAttempts = 0;
        user.LockoutEndUtc = null;
        user.LastLoginAt = now;

        await _unitOfWork.SaveChangesAsync();

        return MapToResponse(user);
    }

    public async Task<AuthUserResponse?> GetUserAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);

        return user == null ? null : MapToResponse(user);
    }

    // ========================================
    // Helpers
    // ========================================

    private async Task RegisterFailedAttemptAsync(AdminUser user, DateTime now)
    {
        user.FailedLoginAttempts++;

        if (user.FailedLoginAttempts >= _options.MaxFailedLoginAttempts)
        {
            user.LockoutEndUtc = now.AddMinutes(_options.LockoutMinutes);
            user.FailedLoginAttempts = 0;

            _logger.LogWarning(
                "Admin account {UserId} locked until {LockoutEnd}",
                user.Id,
                user.LockoutEndUtc);
        }

        await _unitOfWork.SaveChangesAsync();
    }

    private bool IsRegistrationKeyValid(string? providedKey)
    {
        // Registration stays closed until a key is configured
        if (string.IsNullOrEmpty(_options.RegistrationKey) ||
            string.IsNullOrEmpty(providedKey))
        {
            return false;
        }

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(providedKey),
            Encoding.UTF8.GetBytes(_options.RegistrationKey));
    }

    private static string NormalizeEmail(string? email)
    {
        return email?.Trim().ToLowerInvariant() ?? string.Empty;
    }

    private static bool IsValidEmail(string email)
    {
        return email.Length <= EmailMaxLength &&
            MailAddress.TryCreate(email, out var address) &&
            address.Address == email;
    }

    private static void ValidatePassword(string? password)
    {
        if (string.IsNullOrEmpty(password) ||
            password.Length < PasswordMinLength ||
            password.Length > PasswordMaxLength ||
            !password.Any(char.IsUpper) ||
            !password.Any(char.IsLower) ||
            !password.Any(char.IsDigit))
        {
            throw new ArgumentException(
                $"Password must be {PasswordMinLength}-{PasswordMaxLength} characters and contain an upper-case letter, a lower-case letter and a number.");
        }
    }

    private static AuthUserResponse MapToResponse(AdminUser user)
    {
        return new AuthUserResponse
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = AuthConstants.AdminRole
        };
    }
}
