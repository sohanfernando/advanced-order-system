using AdvancedOrderSystem.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace AdvancedOrderSystem.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, message) = exception switch
        {
            ArgumentException =>
                (StatusCodes.Status400BadRequest, exception.Message),

            AuthenticationFailedException =>
                (StatusCodes.Status401Unauthorized, exception.Message),

            ForbiddenException =>
                (StatusCodes.Status403Forbidden, exception.Message),

            KeyNotFoundException =>
                (StatusCodes.Status404NotFound, exception.Message),

            ConflictException =>
                (StatusCodes.Status409Conflict, exception.Message),

            _ =>
                (StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred.")
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception");
        }

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(
            new { message },
            cancellationToken);

        return true;
    }
}
