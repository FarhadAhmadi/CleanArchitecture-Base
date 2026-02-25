using System.Diagnostics;
using SharedKernel;

namespace Web.Api.Infrastructure;

public static class CustomResults
{
    public static IResult Problem(Result result)
    {
        if (result.IsSuccess)
        {
            throw new InvalidOperationException();
        }

        int statusCode = GetStatusCode(result.Error.Type);

        Dictionary<string, object?> extensions = new()
        {
            ["traceId"] = Activity.Current?.TraceId.ToString(),
            ["errorCode"] = result.Error.Code,
            ["timestampUtc"] = DateTime.UtcNow
        };

        if (result.Error is ValidationError validationError)
        {
            extensions["errors"] = validationError.Errors;
        }

        return Results.Problem(
            title: GetTitle(result.Error),
            detail: result.Error.Description,
            type: GetType(statusCode),
            statusCode: statusCode,
            extensions: extensions);
    }

    private static string GetTitle(Error error) =>
        error.Type switch
        {
            ErrorType.Validation => "Validation failed",
            ErrorType.Problem => "Bad request",
            ErrorType.NotFound => "Resource not found",
            ErrorType.Conflict => "Conflict",
            _ => "Server failure"
        };

    private static int GetStatusCode(ErrorType errorType) =>
        errorType switch
        {
            ErrorType.Validation or ErrorType.Problem => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };

    private static string GetType(int statusCode) => $"https://httpstatuses.com/{statusCode}";
}
