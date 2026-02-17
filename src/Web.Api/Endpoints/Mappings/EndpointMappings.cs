using Application.Authorization.AssignPermissionToRole;
using Application.Authorization.AssignRoleToUser;
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
using Web.Api.Endpoints.Authorization;
using Web.Api.Endpoints.Todos;
using Web.Api.Endpoints.Users;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Mappings;

internal static class EndpointMappings
{
    internal static RegisterUserCommand ToCommand(this Register.Request request) =>
        new(
            InputSanitizer.SanitizeEmail(request.Email) ?? string.Empty,
            InputSanitizer.SanitizeText(request.FirstName, 100) ?? string.Empty,
            InputSanitizer.SanitizeText(request.LastName, 100) ?? string.Empty,
            request.Password);

    internal static LoginUserCommand ToCommand(this Login.Request request) =>
        new(
            InputSanitizer.SanitizeEmail(request.Email) ?? string.Empty,
            request.Password);

    internal static RefreshTokenCommand ToCommand(this Refresh.Request request) =>
        new(request.RefreshToken.Trim());

    internal static RevokeRefreshTokenCommand ToCommand(this Revoke.Request request) =>
        new(request.RefreshToken.Trim());

    internal static CreateTodoCommand ToCommand(this Create.Request request, Guid userId) =>
        new()
        {
            UserId = userId,
            Description = InputSanitizer.SanitizeText(request.Description, 1000) ?? string.Empty,
            DueDate = request.DueDate,
            Labels = InputSanitizer.SanitizeList(request.Labels, 100),
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
        new(
            userId,
            page,
            pageSize,
            InputSanitizer.SanitizeText(search, 200),
            isCompleted,
            InputSanitizer.SanitizeIdentifier(sortBy, 50),
            InputSanitizer.SanitizeIdentifier(sortOrder, 10));

    internal static GetTodoByIdQuery ToGetTodoByIdQuery(this Guid todoId) =>
        new(todoId);

    internal static DeleteTodoCommand ToDeleteTodoCommand(this Guid todoId) =>
        new(todoId);

    internal static CompleteTodoCommand ToCompleteTodoCommand(this Guid todoId) =>
        new(todoId);

    internal static GetUserByIdQuery ToGetUserByIdQuery(this Guid userId) =>
        new(userId);

    internal static AssignRoleToUserCommand ToCommand(this AssignRoleToUser.Request request) =>
        new(
            request.UserId,
            InputSanitizer.SanitizeIdentifier(request.RoleName, 100) ?? string.Empty);

    internal static AssignPermissionToRoleCommand ToCommand(this AssignPermissionToRole.Request request) =>
        new(
            InputSanitizer.SanitizeIdentifier(request.RoleName, 100) ?? string.Empty,
            InputSanitizer.SanitizeIdentifier(request.PermissionCode, 200) ?? string.Empty);

}
