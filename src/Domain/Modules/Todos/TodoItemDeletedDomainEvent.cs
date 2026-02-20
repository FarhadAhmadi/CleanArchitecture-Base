using SharedKernel;

namespace Domain.Todos;

public sealed record TodoItemDeletedDomainEvent(Guid TodoItemId) : IVersionedDomainEvent
{
    public string ContractName => "todos.todo-item-deleted";
    public int ContractVersion => 1;
}
