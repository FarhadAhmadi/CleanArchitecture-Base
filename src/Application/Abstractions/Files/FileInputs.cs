namespace Application.Abstractions.Files;

public sealed class UploadFileInput
{
    public required IFormFile File { get; init; }
    public string Module { get; init; } = "General";
    public string? Folder { get; init; }
    public string? Description { get; init; }
}

public sealed class ScanFileInput
{
    public required IFormFile File { get; init; }
    public string Module { get; init; } = "General";
    public string? Folder { get; init; }
    public string? Description { get; init; }
}

public sealed record ValidateFileInput(string FileName, long SizeBytes, string? ContentType);
public sealed record UpdateFileMetadataInput(string FileName, string? Description);
public sealed record MoveFileInput(string Module, string? Folder);
public sealed record AddFileTagInput(string Tag);
public sealed record UpsertFilePermissionInput(string SubjectType, string SubjectValue, bool CanRead, bool CanWrite, bool CanDelete);
public sealed record SearchFilesInput(string? Query, string? FileType, Guid? UploaderId, DateTime? From, DateTime? To, int Page, int PageSize);
public sealed record FilterFilesInput(string? Module, int Page, int PageSize);
