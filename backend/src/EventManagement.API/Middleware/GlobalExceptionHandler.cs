using EventManagement.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace EventManagement.API.Middleware;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var correlationId = context.Response.Headers["X-Correlation-Id"].FirstOrDefault()
            ?? context.TraceIdentifier;

        // Log exception type and correlation ID only — NO PII in log messages
        logger.LogError(exception,
            "Unhandled exception {ExceptionType} | CorrelationId: {CorrelationId}",
            exception.GetType().Name, correlationId);

        var (status, title, detail) = exception switch
        {
            DuplicateEmailException => (409, "Conflict", exception.Message),
            VerificationTokenExpiredException => (400, "Token Expired", exception.Message),
            InvalidVerificationTokenException => (400, "Invalid Token", exception.Message),
            AuthenticationFailedException => (401, "Unauthorized", exception.Message),
            InvalidRefreshTokenException => (401, "Unauthorized", exception.Message),
            FluentValidation.ValidationException => (422, "Validation Failed", "One or more validation errors occurred."),
            _ => (500, "Internal Server Error", "An unexpected error occurred. Please try again later.")
        };

        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";

        if (exception is FluentValidation.ValidationException valEx)
        {
            var errors = valEx.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            var validationProblem = new ValidationProblemDetails(errors)
            {
                Status = 422,
                Title = "Validation Failed",
                Detail = "One or more validation errors occurred.",
                Type = "https://httpstatuses.io/422"
            };
            validationProblem.Extensions["correlationId"] = correlationId;

            await context.Response.WriteAsJsonAsync(validationProblem, cancellationToken);
        }
        else
        {
            var problem = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = status < 500 ? detail : "An unexpected error occurred. Please try again later.",
                Type = $"https://httpstatuses.io/{status}"
            };
            problem.Extensions["correlationId"] = correlationId;

            await context.Response.WriteAsJsonAsync(problem, cancellationToken);
        }

        return true;
    }
}
