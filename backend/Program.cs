using System.Threading.RateLimiting;
using AdvancedOrderSystem.Auth;
using AdvancedOrderSystem.Data;
using AdvancedOrderSystem.Middleware;
using AdvancedOrderSystem.Models.Entities;
using AdvancedOrderSystem.Repositories;
using AdvancedOrderSystem.Repositories.Impl;
using AdvancedOrderSystem.Services;
using AdvancedOrderSystem.Services.Impl;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

const string FrontendCorsPolicy = "Frontend";

// Load .env into environment variables before configuration is built.
// NoClobber keeps variables that are already set in the real environment.
DotNetEnv.Env.NoClobber().TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddOpenApi();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// =========================
// CORS
// =========================

var allowedOrigins =
    builder.Configuration
        .GetSection("Cors:AllowedOrigins")
        .Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            // Needed so the browser sends the auth cookie
            .AllowCredentials();
    });
});

// =========================
// Authentication
// =========================

builder.Services.Configure<AuthOptions>(
    builder.Configuration.GetSection(AuthOptions.SectionName));

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = AuthConstants.CookieName;
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;

        options.ExpireTimeSpan = AuthConstants.SessionDuration;
        options.SlidingExpiration = true;

        // An API answers with status codes instead of redirecting to a login page
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };

        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    });

// =========================
// Authorization
// =========================

var adminPolicy =
    new AuthorizationPolicyBuilder(
            CookieAuthenticationDefaults.AuthenticationScheme)
        .RequireAuthenticatedUser()
        .RequireRole(AuthConstants.AdminRole)
        .Build();

builder.Services.AddAuthorizationBuilder()
    .AddPolicy(AuthConstants.AdminPolicy, adminPolicy)
    // Every endpoint requires an admin unless it is marked [AllowAnonymous]
    .SetFallbackPolicy(adminPolicy);

// =========================
// Rate limiting
// =========================

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Sign-in / sign-up: 10 requests per minute per IP address
    options.AddPolicy(AuthConstants.AuthRateLimitPolicy, httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey:
                httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));

    options.OnRejected = async (context, cancellationToken) =>
    {
        await context.HttpContext.Response.WriteAsJsonAsync(
            new { message = "Too many attempts. Please wait a minute and try again." },
            cancellationToken);
    };
});

// =========================
// Database
// =========================

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    );
});

// =========================
// Application services
// =========================

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<IPasswordHasher<AdminUser>, PasswordHasher<AdminUser>>();

builder.Services.AddScoped<IAdminUserRepository, AdminUserRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();

var app = builder.Build();

app.UseExceptionHandler();

// CORS runs before the HTTPS redirect and auth so preflight requests succeed
app.UseCors(FrontendCorsPolicy);

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseRateLimiter();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();
}

app.MapControllers();

app.Run();
