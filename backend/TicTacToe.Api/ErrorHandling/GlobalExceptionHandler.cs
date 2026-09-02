using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TicTacToe.Application.Exceptions;
using TicTacToe.Domain;

namespace TicTacToe.Api.ErrorHandling;

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IHostEnvironment environment) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (status, title, type) = exception switch
        {
            ValidationException => (StatusCodes.Status400BadRequest, "Validation failed", "validation-error"),
            ConcurrencyConflictException => (StatusCodes.Status409Conflict, "Concurrency conflict", "concurrency-conflict"),
            DomainInvariantException => (StatusCodes.Status500InternalServerError, "Domain invariant violated", "domain-invariant"),
            _ => (StatusCodes.Status500InternalServerError, "Internal server error", "server-error")
        };

        if (status >= 500)
            logger.LogError(exception, "Unhandled exception for {Path}", httpContext.Request.Path);
        else
            logger.LogWarning(exception, "Handled application exception for {Path}", httpContext.Request.Path);

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Type = $"https://localhost/errors/{type}",
            Instance = httpContext.Request.Path
        };

        problem.Extensions["traceId"] = httpContext.TraceIdentifier;
        if (status < 500 || environment.IsDevelopment())
            problem.Detail = exception is ValidationException validation
                ? string.Join("; ", validation.Errors.Select(x => x.ErrorMessage))
                : exception.Message;

        httpContext.Response.StatusCode = status;
        httpContext.Response.ContentType = "application/problem+json";
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
        return true;
    }
}
