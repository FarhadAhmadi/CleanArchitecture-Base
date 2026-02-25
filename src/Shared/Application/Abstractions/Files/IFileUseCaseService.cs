namespace Application.Abstractions.Files;

public interface IFileUseCaseService
{
    Task<IResult> UploadAsync(UploadFileInput input, HttpContext httpContext, CancellationToken cancellationToken);
    IResult Validate(ValidateFileInput input);
    Task<IResult> ScanAsync(ScanFileInput input, CancellationToken cancellationToken);
    Task<IResult> GetMetadataAsync(Guid fileId, CancellationToken cancellationToken);
    Task<IResult> DownloadAsync(Guid fileId, HttpContext httpContext, CancellationToken cancellationToken);
    Task<IResult> StreamAsync(Guid fileId, HttpContext httpContext, CancellationToken cancellationToken);
    Task<IResult> GetSecureLinkAsync(Guid fileId, string? mode, HttpContext httpContext, CancellationToken cancellationToken);
    Task<IResult> ShareAsync(Guid fileId, string? mode, HttpContext httpContext, CancellationToken cancellationToken);
    Task<IResult> GetPublicByLinkAsync(string token, HttpContext httpContext, CancellationToken cancellationToken);
    Task<IResult> GetAuditAsync(Guid fileId, CancellationToken cancellationToken);
    Task<IResult> DeleteAsync(Guid fileId, HttpContext httpContext, CancellationToken cancellationToken);
    Task<IResult> UpdateMetadataAsync(Guid fileId, UpdateFileMetadataInput input, CancellationToken cancellationToken);
    Task<IResult> MoveAsync(Guid fileId, MoveFileInput input, CancellationToken cancellationToken);
    Task<IResult> SearchAsync(SearchFilesInput input, CancellationToken cancellationToken);
    Task<IResult> FilterAsync(FilterFilesInput input, CancellationToken cancellationToken);
    Task<IResult> AddTagAsync(Guid fileId, AddFileTagInput input, CancellationToken cancellationToken);
    Task<IResult> SearchByTagAsync(string tag, CancellationToken cancellationToken);
    Task<IResult> UpsertPermissionAsync(Guid fileId, UpsertFilePermissionInput input, CancellationToken cancellationToken);
    Task<IResult> GetPermissionsAsync(Guid fileId, CancellationToken cancellationToken);
    Task<IResult> SetEncryptedAsync(Guid fileId, CancellationToken cancellationToken);
    Task<IResult> SetDecryptedAsync(Guid fileId, CancellationToken cancellationToken);
}
