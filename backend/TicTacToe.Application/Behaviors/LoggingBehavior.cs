using MediatR;
using Microsoft.Extensions.Logging;

namespace TicTacToe.Application.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        logger.LogInformation("Handling MediatR request {RequestName}", requestName);

        try
        {
            var response = await next(cancellationToken);
            logger.LogInformation("Handled MediatR request {RequestName}", requestName);
            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "MediatR request {RequestName} failed", requestName);
            throw;
        }
    }
}
