using System.Diagnostics;
using Application.Abstractions.Messaging;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using SharedKernel;

namespace Application.Abstractions.Behaviors;

internal static class LoggingDecorator
{
    private const long SlowOperationThresholdMs = 500;

    internal sealed class CommandHandler<TCommand, TResponse>(
        ICommandHandler<TCommand, TResponse> innerHandler,
        ILogger<CommandHandler<TCommand, TResponse>> logger)
        : ICommandHandler<TCommand, TResponse>
        where TCommand : ICommand<TResponse>
    {
        public async Task<Result<TResponse>> Handle(TCommand command, CancellationToken cancellationToken)
        {
            string commandName = typeof(TCommand).Name;
            string operationId = Guid.NewGuid().ToString("N");
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                    "Processing command {Command} OperationId={OperationId} Canceled={Canceled}",
                    commandName,
                    operationId,
                    cancellationToken.IsCancellationRequested);
            }

            var stopwatch = Stopwatch.StartNew();
            Result<TResponse> result = await innerHandler.Handle(command, cancellationToken);
            stopwatch.Stop();

            if (result.IsSuccess)
            {
                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.LogInformation(
                        "Completed command {Command} OperationId={OperationId} DurationMs={DurationMs}",
                        commandName,
                        operationId,
                        stopwatch.ElapsedMilliseconds);
                }

                if (stopwatch.ElapsedMilliseconds >= SlowOperationThresholdMs && logger.IsEnabled(LogLevel.Warning))
                {
                    logger.LogWarning(
                        "Slow command detected {Command} OperationId={OperationId} DurationMs={DurationMs}",
                        commandName,
                        operationId,
                        stopwatch.ElapsedMilliseconds);
                }
            }
            else
            {
                using (LogContext.PushProperty("Error", result.Error, true))
                {
                    if (logger.IsEnabled(LogLevel.Error))
                    {
                        logger.LogError(
                            "Command failed {Command} OperationId={OperationId} ErrorCode={ErrorCode} ErrorType={ErrorType} DurationMs={DurationMs}",
                            commandName,
                            operationId,
                            result.Error.Code,
                            result.Error.Type,
                            stopwatch.ElapsedMilliseconds);
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
            string operationId = Guid.NewGuid().ToString("N");
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                    "Processing command {Command} OperationId={OperationId} Canceled={Canceled}",
                    commandName,
                    operationId,
                    cancellationToken.IsCancellationRequested);
            }

            var stopwatch = Stopwatch.StartNew();
            Result result = await innerHandler.Handle(command, cancellationToken);
            stopwatch.Stop();

            if (result.IsSuccess)
            {
                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.LogInformation(
                        "Completed command {Command} OperationId={OperationId} DurationMs={DurationMs}",
                        commandName,
                        operationId,
                        stopwatch.ElapsedMilliseconds);
                }

                if (stopwatch.ElapsedMilliseconds >= SlowOperationThresholdMs && logger.IsEnabled(LogLevel.Warning))
                {
                    logger.LogWarning(
                        "Slow command detected {Command} OperationId={OperationId} DurationMs={DurationMs}",
                        commandName,
                        operationId,
                        stopwatch.ElapsedMilliseconds);
                }
            }
            else
            {
                using (LogContext.PushProperty("Error", result.Error, true))
                {
                    if (logger.IsEnabled(LogLevel.Error))
                    {
                        logger.LogError(
                            "Command failed {Command} OperationId={OperationId} ErrorCode={ErrorCode} ErrorType={ErrorType} DurationMs={DurationMs}",
                            commandName,
                            operationId,
                            result.Error.Code,
                            result.Error.Type,
                            stopwatch.ElapsedMilliseconds);
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
            string operationId = Guid.NewGuid().ToString("N");
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                    "Processing query {Query} OperationId={OperationId} Canceled={Canceled}",
                    queryName,
                    operationId,
                    cancellationToken.IsCancellationRequested);
            }

            var stopwatch = Stopwatch.StartNew();
            Result<TResponse> result = await innerHandler.Handle(query, cancellationToken);
            stopwatch.Stop();

            if (result.IsSuccess)
            {
                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.LogInformation(
                        "Completed query {Query} OperationId={OperationId} DurationMs={DurationMs}",
                        queryName,
                        operationId,
                        stopwatch.ElapsedMilliseconds);
                }

                if (stopwatch.ElapsedMilliseconds >= SlowOperationThresholdMs && logger.IsEnabled(LogLevel.Warning))
                {
                    logger.LogWarning(
                        "Slow query detected {Query} OperationId={OperationId} DurationMs={DurationMs}",
                        queryName,
                        operationId,
                        stopwatch.ElapsedMilliseconds);
                }
            }
            else
            {
                using (LogContext.PushProperty("Error", result.Error, true))
                {
                    if (logger.IsEnabled(LogLevel.Error))
                    {
                        logger.LogError(
                            "Query failed {Query} OperationId={OperationId} ErrorCode={ErrorCode} ErrorType={ErrorType} DurationMs={DurationMs}",
                            queryName,
                            operationId,
                            result.Error.Code,
                            result.Error.Type,
                            stopwatch.ElapsedMilliseconds);
                    }
                }
            }

            return result;
        }
    }
}
