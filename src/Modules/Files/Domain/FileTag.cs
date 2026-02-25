namespace Domain.Files;

public sealed class FileTag
{
    public Guid FileId { get; set; }
    public string Tag { get; set; } = string.Empty;
}
