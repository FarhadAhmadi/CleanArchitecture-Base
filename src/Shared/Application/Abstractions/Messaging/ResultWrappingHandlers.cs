using SharedKernel;

namespace Application.Abstractions.Messaging;

public abstract class ResultWrappingCommandHandler<TCommand> : ICommandHandler<TCommand, IResult>
    where TCommand : ICommand<IResult>
{
    public async Task<Result<IResult>> Handle(TCommand command, CancellationToken cancellationToken)
    {
        IResult result = await HandleCore(command, cancellationToken);
        return Result.Success(result);
    }

    protected abstract Task<IResult> HandleCore(TCommand command, CancellationToken cancellationToken);
}

public abstract class ResultWrappingQueryHandler<TQuery> : IQueryHandler<TQuery, IResult>
    where TQuery : IQuery<IResult>
{
    public async Task<Result<IResult>> Handle(TQuery query, CancellationToken cancellationToken)
    {
        IResult result = await HandleCore(query, cancellationToken);
        return Result.Success(result);
    }

    protected abstract Task<IResult> HandleCore(TQuery query, CancellationToken cancellationToken);
}
