using Application.Todos.Complete;
using Application.Todos.Copy;
using Application.Todos.Create;
using Application.Todos.Delete;
using Application.Todos.Get;
using Application.Todos.GetById;
using Application.Users.GetById;
using Application.Users.Login;
using Application.Users.Register;
using Application.Users.Tokens;
using Domain.Todos;
using Web.Api.Endpoints.Todos;
using Web.Api.Endpoints.Users;

namespace Web.Api.Endpoints.Mappings;

internal static class EndpointMappings
{
    internal static RegisterUserCommand ToCommand(this Register.Request request) =>
        new(request.Email, request.FirstName, request.LastName, request.Password);

    internal static LoginUserCommand ToCommand(this Login.Request request) =>
        new(request.Email, request.Password);

    internal static RefreshTokenCommand ToCommand(this Refresh.Request request) =>
        new(request.RefreshToken);

    internal static RevokeRefreshTokenCommand ToCommand(this Revoke.Request request) =>
        new(request.RefreshToken);

    internal static CreateTodoCommand ToCommand(this Create.Request request, Guid userId) =>
        new()
        {
            UserId = userId,
            Description = request.Description,
            DueDate = request.DueDate,
            Labels = request.Labels,
            Priority = (Priority)request.Priority
        };

    internal static CopyTodoCommand ToCopyTodoCommand(this Guid todoId, Guid userId) =>
        new()
        {
            UserId = userId,
            TodoId = todoId
        };

    internal static GetTodosQuery ToGetTodosQuery(
        this Guid userId,
        int page,
        int pageSize,
        string? search,
        bool? isCompleted,
        string? sortBy,
        string? sortOrder) =>
        new(userId, page, pageSize, search, isCompleted, sortBy, sortOrder);

    internal static GetTodoByIdQuery ToGetTodoByIdQuery(this Guid todoId) =>
        new(todoId);

    internal static DeleteTodoCommand ToDeleteTodoCommand(this Guid todoId) =>
        new(todoId);

    internal static CompleteTodoCommand ToCompleteTodoCommand(this Guid todoId) =>
        new(todoId);

    internal static GetUserByIdQuery ToGetUserByIdQuery(this Guid userId) =>
        new(userId);
}
