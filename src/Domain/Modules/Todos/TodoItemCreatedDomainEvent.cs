using SharedKernel;

namespace Domain.Todos;

public sealed record TodoItemCreatedDomainEvent(Guid TodoItemId) : IVersionedDomainEvent
{
    public string ContractName => "todos.todo-item-created";
    public int ContractVersion => 1;
}
