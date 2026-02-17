using System.Diagnostics;
using Application.Abstractions.Messaging;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using SharedKernel;

namespace Application.Abstractions.Behaviors;

internal static class LoggingDecorator
{
    internal sealed class CommandHandler<TCommand, TResponse>(
        ICommandHandler<TCommand, TResponse> innerHandler,
        ILogger<CommandHandler<TCommand, TResponse>> logger)
        : ICommandHandler<TCommand, TResponse>
        where TCommand : ICommand<TResponse>
    {
        public async Task<Result<TResponse>> Handle(TCommand command, CancellationToken cancellationToken)
        {
            string commandName = typeof(TCommand).Name;
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Processing command {Command}", commandName);
            }

            var stopwatch = Stopwatch.StartNew();
            Result<TResponse> result = await innerHandler.Handle(command, cancellationToken);
            stopwatch.Stop();

            if (result.IsSuccess)
            {
                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.LogInformation("Completed command {Command} DurationMs={DurationMs}", commandName, stopwatch.ElapsedMilliseconds);
                }
            }
            else
            {
                using (LogContext.PushProperty("Error", result.Error, true))
                {
                    if (logger.IsEnabled(LogLevel.Error))
                    {
                        logger.LogError("Command failed {Command} DurationMs={DurationMs}", commandName, stopwatch.ElapsedMilliseconds);
                    }
                }
            }

            return result;
        }
    }

    internal sealed class CommandBaseHandler<TCommand>(
        ICommandHandler<TCommand> innerHandler,
        ILogger<CommandBaseHandler<TCommand>> logger)
        : ICommandHandler<TCommand>
        where TCommand : ICommand
    {
        public async Task<Result> Handle(TCommand command, CancellationToken cancellationToken)
        {
            string commandName = typeof(TCommand).Name;
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Processing command {Command}", commandName);
            }

            var stopwatch = Stopwatch.StartNew();
            Result result = await innerHandler.Handle(command, cancellationToken);
            stopwatch.Stop();

            if (result.IsSuccess)
            {
                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.LogInformation("Completed command {Command} DurationMs={DurationMs}", commandName, stopwatch.ElapsedMilliseconds);
                }
            }
            else
            {
                using (LogContext.PushProperty("Error", result.Error, true))
                {
                    if (logger.IsEnabled(LogLevel.Error))
                    {
                        logger.LogError("Command failed {Command} DurationMs={DurationMs}", commandName, stopwatch.ElapsedMilliseconds);
                    }
                }
            }

            return result;
        }
    }

    internal sealed class QueryHandler<TQuery, TResponse>(
        IQueryHandler<TQuery, TResponse> innerHandler,
        ILogger<QueryHandler<TQuery, TResponse>> logger)
        : IQueryHandler<TQuery, TResponse>
        where TQuery : IQuery<TResponse>
    {
        public async Task<Result<TResponse>> Handle(TQuery query, CancellationToken cancellationToken)
        {
            string queryName = typeof(TQuery).Name;
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Processing query {Query}", queryName);
            }

            var stopwatch = Stopwatch.StartNew();
            Result<TResponse> result = await innerHandler.Handle(query, cancellationToken);
            stopwatch.Stop();

            if (result.IsSuccess)
            {
                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.LogInformation("Completed query {Query} DurationMs={DurationMs}", queryName, stopwatch.ElapsedMilliseconds);
                }
            }
            else
            {
                using (LogContext.PushProperty("Error", result.Error, true))
                {
                    if (logger.IsEnabled(LogLevel.Error))
                    {
                        logger.LogError("Query failed {Query} DurationMs={DurationMs}", queryName, stopwatch.ElapsedMilliseconds);
                    }
                }
            }

            return result;
        }
    }
}
