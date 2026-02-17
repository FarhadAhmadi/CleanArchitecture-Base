using Application.Todos.Copy;
using Application.Todos.Create;
using Application.Users.Login;
using Application.Users.Register;
using Shouldly;
using Web.Api.Endpoints.Mappings;
using Web.Api.Endpoints.Todos;
using Web.Api.Endpoints.Users;

namespace ArchitectureTests.Mappings;

public sealed class EndpointMappingsTests
{
    [Fact]
    public void RegisterRequest_ShouldMapToCommand()
    {
        var request = new Register.Request("a@test.local", "A", "B", "Pass123!");

        RegisterUserCommand command = request.ToCommand();

        command.Email.ShouldBe("a@test.local");
        command.FirstName.ShouldBe("A");
        command.LastName.ShouldBe("B");
        command.Password.ShouldBe("Pass123!");
    }

    [Fact]
    public void LoginRequest_ShouldMapToCommand()
    {
        var request = new Login.Request("a@test.local", "Pass123!");

        LoginUserCommand command = request.ToCommand();

        command.Email.ShouldBe("a@test.local");
        command.Password.ShouldBe("Pass123!");
    }

    [Fact]
    public void CreateTodoRequest_ShouldMapToCommand()
    {
        var request = new Create.Request
        {
            Description = "hello",
            Priority = 1,
            Labels = ["one"]
        };

        var userId = Guid.NewGuid();

        CreateTodoCommand command = request.ToCommand(userId);

        command.UserId.ShouldBe(userId);
        command.Description.ShouldBe("hello");
        command.Priority.ShouldBe((Domain.Todos.Priority)1);
        command.Labels.Count.ShouldBe(1);
    }

    [Fact]
    public void TodoId_ShouldMapToCopyCommand()
    {
        var userId = Guid.NewGuid();
        var todoId = Guid.NewGuid();

        var command = todoId.ToCopyTodoCommand(userId);

        command.UserId.ShouldBe(userId);
        command.TodoId.ShouldBe(todoId);
    }
}
