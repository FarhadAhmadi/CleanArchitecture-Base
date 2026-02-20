using SharedKernel;

namespace Domain.Todos;

public sealed record TodoItemCompletedDomainEvent(Guid TodoItemId) : IVersionedDomainEvent
{
    public string ContractName => "todos.todo-item-completed";
    public int ContractVersion => 1;
}
