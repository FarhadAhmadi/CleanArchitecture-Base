namespace Infrastructure.Files;

public sealed class FileValidationOptions
{
    public const string SectionName = "FileValidation";

    public int MaxFileSizeMb { get; init; } = 50;
    public string[] AllowedExtensions { get; init; } =
    [
        ".jpg", ".jpeg", ".png", ".pdf", ".docx", ".zip", ".rar", ".mp4", ".mov", ".txt"
    ];
}
