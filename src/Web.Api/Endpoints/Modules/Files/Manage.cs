using System.Security.Cryptography;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Domain.Files;
using Infrastructure.Files;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using Web.Api.Endpoints.Users;
using Web.Api.Extensions;

namespace Web.Api.Endpoints.Files;

internal sealed class Manage : IEndpoint
{
    private const string SubjectTypeUser = "User";
    private const string SubjectTypeRole = "Role";
    private const string LinkModeInline = "inline";
    private const string LinkModeDownload = "download";
    private const string LinkModeStream = "stream";

    public sealed class UploadForm
    {
        [FromForm(Name = "file")]
        public IFormFile File { get; set; } = default!;

        [FromForm(Name = "module")]
        public string Module { get; set; } = "General";

        [FromForm(Name = "folder")]
        public string? Folder { get; set; }

        [FromForm(Name = "description")]
        public string? Description { get; set; }
    }

    public sealed record ValidateRequest(string FileName, long SizeBytes, string? ContentType);
    public sealed record TagRequest(string Tag);
    public sealed record MetadataRequest(string FileName, string? Description);
    public sealed record MoveRequest(string Module, string? Folder);
    public sealed record PermissionRequest(string SubjectType, string SubjectValue, bool CanRead, bool CanWrite, bool CanDelete);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("files").WithTags(Tags.Files);

        group.MapPost("/", UploadAsync).HasPermission(Permissions.FilesWrite).RequireRateLimiting(Web.Api.DependencyInjection.GetRateLimiterPolicyName());
        group.MapPost("/validate", Validate).HasPermission(Permissions.FilesWrite);
        group.MapPost("/scan", ScanAsync).HasPermission(Permissions.FilesWrite);

        group.MapGet("/{fileId:guid}", GetMetadataAsync).HasPermission(Permissions.FilesRead);
        group.MapGet("/{fileId:guid}/download", DownloadAsync).HasPermission(Permissions.FilesRead);
        group.MapGet("/{fileId:guid}/stream", StreamAsync).HasPermission(Permissions.FilesRead);
        group.MapGet("/{fileId:guid}/link", GetSecureLinkAsync).HasPermission(Permissions.FilesShare);
        group.MapPost("/{fileId:guid}/share", ShareAsync).HasPermission(Permissions.FilesShare);
        group.MapGet("/public/{token}", GetPublicByLinkAsync);
        group.MapGet("/audit/{fileId:guid}", GetAuditAsync).HasPermission(Permissions.FilesRead);

        group.MapDelete("/{fileId:guid}", DeleteAsync).HasPermission(Permissions.FilesDelete);
        group.MapPut("/{fileId:guid}", UpdateMetadataAsync).HasPermission(Permissions.FilesWrite);
        group.MapPatch("/{fileId:guid}/move", MoveAsync).HasPermission(Permissions.FilesWrite);

        group.MapGet("/search", SearchAsync).HasPermission(Permissions.FilesRead);
        group.MapGet("/filter", FilterAsync).HasPermission(Permissions.FilesRead);
        group.MapPost("/{fileId:guid}/tags", AddTagAsync).HasPermission(Permissions.FilesWrite);
        group.MapGet("/tags/{tag}", SearchByTagAsync).HasPermission(Permissions.FilesRead);

        group.MapPost("/permissions/{fileId:guid}", UpsertPermissionAsync).HasPermission(Permissions.FilesPermissionsManage);
        group.MapGet("/permissions/{fileId:guid}", GetPermissionsAsync).HasPermission(Permissions.FilesPermissionsManage);
        group.MapPost("/encrypt/{fileId:guid}", SetEncryptedAsync).HasPermission(Permissions.FilesPermissionsManage);
        group.MapPost("/decrypt/{fileId:guid}", SetDecryptedAsync).HasPermission(Permissions.FilesPermissionsManage);
    }

    private static async Task<IResult> UploadAsync(
        [FromForm] UploadForm request,
        IUserContext userContext,
        HttpContext httpContext,
        IApplicationDbContext writeContext,
        IFileObjectStorage storage,
        IFileMalwareScanner scanner,
        FileValidationOptions validationOptions,
        FileStorageOptions storageOptions,
        CancellationToken cancellationToken)
    {
        if (request.File.Length == 0)
        {
            return Results.BadRequest(new { message = "Empty file is not allowed." });
        }

        (bool isValid, string validationMessage) = ValidateFile(request.File.FileName, request.File.Length, request.File.ContentType, validationOptions);
        if (!isValid)
        {
            return Results.BadRequest(new { message = validationMessage });
        }

        string safeFileName = SanitizeFileName(request.File.FileName);
        string extension = Path.GetExtension(safeFileName).ToUpperInvariant();
        string objectKey = BuildObjectKey(storageOptions.ObjectPrefix, request.Module, extension);

        await using Stream source = request.File.OpenReadStream();
        await using var copy = new MemoryStream((int)request.File.Length);
        await source.CopyToAsync(copy, cancellationToken);
        copy.Position = 0;

        FileScanResult scanResult = await scanner.ScanAsync(copy, cancellationToken);
        if (!scanResult.IsClean)
        {
            return Results.BadRequest(new { message = "Malicious file detected.", details = scanResult.Message });
        }

        copy.Position = 0;
        string sha256 = ComputeSha256(copy);
        copy.Position = 0;

        await storage.UploadAsync(objectKey, copy, copy.Length, request.File.ContentType ?? "application/octet-stream", cancellationToken);

        var entity = new FileAsset
        {
            Id = Guid.NewGuid(),
            OwnerUserId = userContext.UserId,
            ObjectKey = objectKey,
            FileName = safeFileName,
            Extension = extension,
            ContentType = request.File.ContentType ?? "application/octet-stream",
            SizeBytes = request.File.Length,
            Module = string.IsNullOrWhiteSpace(request.Module) ? "General" : request.Module.Trim(),
            Folder = string.IsNullOrWhiteSpace(request.Folder) ? null : request.Folder.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            Sha256 = sha256,
            IsScanned = true,
            UploadedAtUtc = DateTime.UtcNow
        };

        writeContext.FileAssets.Add(entity);
        await WriteAccessAuditAsync(writeContext, httpContext, entity.Id, userContext.UserId, "upload", cancellationToken);

        return Results.Ok(new { fileId = entity.Id, entity.FileName, entity.SizeBytes, entity.ContentType, entity.Module, entity.UploadedAtUtc });
    }

    private static IResult Validate(ValidateRequest request, FileValidationOptions options)
    {
        (bool isValid, string message) = ValidateFile(request.FileName, request.SizeBytes, request.ContentType, options);
        return Results.Ok(new { isValid, message });
    }

    private static async Task<IResult> ScanAsync([FromForm] UploadForm request, IFileMalwareScanner scanner, CancellationToken cancellationToken)
    {
        await using Stream stream = request.File.OpenReadStream();
        FileScanResult result = await scanner.ScanAsync(stream, cancellationToken);
        return Results.Ok(new { result.IsClean, result.Message });
    }

    private static async Task<IResult> GetMetadataAsync(Guid fileId, IUserContext userContext, IApplicationReadDbContext readContext, CancellationToken cancellationToken)
    {
        FileAsset? file = await readContext.FileAssets.SingleOrDefaultAsync(x => x.Id == fileId && !x.IsDeleted, cancellationToken);
        if (file is null)
        {
            return Results.NotFound();
        }

        if (!await HasReadAccessAsync(file, userContext.UserId, readContext, cancellationToken))
        {
            return Results.Forbid();
        }

        return Results.Ok(new { file.Id, file.FileName, file.ContentType, file.SizeBytes, file.Module, file.Folder, file.Description, file.OwnerUserId, file.UploadedAtUtc, file.UpdatedAtUtc, file.IsEncrypted });
    }

    private static async Task<IResult> DownloadAsync(
        Guid fileId,
        IUserContext userContext,
        HttpContext httpContext,
        IApplicationReadDbContext readContext,
        IApplicationDbContext writeContext,
        IFileObjectStorage storage,
        CancellationToken cancellationToken)
    {
        FileAsset? file = await readContext.FileAssets.SingleOrDefaultAsync(x => x.Id == fileId && !x.IsDeleted, cancellationToken);
        if (file is null)
        {
            return Results.NotFound();
        }

        if (!await HasOwnerOrAdminAccessAsync(file, userContext.UserId, readContext, cancellationToken))
        {
            return Results.Forbid();
        }

        (Stream content, string contentType) = await storage.OpenReadAsync(file.ObjectKey, cancellationToken);
        await WriteAccessAuditAsync(writeContext, httpContext, file.Id, userContext.UserId, "download", cancellationToken);

        return Results.File(
            content,
            contentType,
            fileDownloadName: file.FileName,
            enableRangeProcessing: true);
    }

    private static Task<IResult> StreamAsync(
        Guid fileId,
        IUserContext userContext,
        HttpContext httpContext,
        IApplicationReadDbContext readContext,
        IApplicationDbContext writeContext,
        IFileObjectStorage storage,
        CancellationToken cancellationToken)
    {
        return StreamInternalAsync(fileId, userContext, httpContext, readContext, writeContext, storage, cancellationToken);
    }

    private static async Task<IResult> StreamInternalAsync(
        Guid fileId,
        IUserContext userContext,
        HttpContext httpContext,
        IApplicationReadDbContext readContext,
        IApplicationDbContext writeContext,
        IFileObjectStorage storage,
        CancellationToken cancellationToken)
    {
        FileAsset? file = await readContext.FileAssets.SingleOrDefaultAsync(x => x.Id == fileId && !x.IsDeleted, cancellationToken);
        if (file is null)
        {
            return Results.NotFound();
        }

        if (!await HasOwnerOrAdminAccessAsync(file, userContext.UserId, readContext, cancellationToken))
        {
            return Results.Forbid();
        }

        (Stream content, string contentType) = await storage.OpenReadAsync(file.ObjectKey, cancellationToken);
        await WriteAccessAuditAsync(writeContext, httpContext, file.Id, userContext.UserId, "stream", cancellationToken);

        httpContext.Response.Headers[HeaderNames.ContentDisposition] = $"inline; filename=\"{file.FileName}\"";

        return Results.File(
            content,
            contentType,
            fileDownloadName: null,
            enableRangeProcessing: true);
    }

    private static async Task<IResult> GetSecureLinkAsync(
        Guid fileId,
        string? mode,
        IUserContext userContext,
        HttpContext httpContext,
        IApplicationReadDbContext readContext,
        FileAppLinkService linkService,
        FileStorageOptions storageOptions,
        CancellationToken cancellationToken)
    {
        FileAsset? file = await readContext.FileAssets.SingleOrDefaultAsync(x => x.Id == fileId && !x.IsDeleted, cancellationToken);
        if (file is null)
        {
            return Results.NotFound();
        }

        if (!await HasOwnerOrAdminAccessAsync(file, userContext.UserId, readContext, cancellationToken))
        {
            return Results.Forbid();
        }

        string normalizedMode = NormalizeLinkMode(mode);
        string token = linkService.CreateToken(file.Id, normalizedMode, DateTime.UtcNow);
        string url = BuildAppLinkUrl(httpContext, token, normalizedMode);

        return Results.Ok(new
        {
            url,
            mode = normalizedMode,
            expiresAtUtc = DateTime.UtcNow.AddMinutes(Math.Max(1, storageOptions.AppLinkExpiryMinutes))
        });
    }

    private static Task<IResult> ShareAsync(
        Guid fileId,
        string? mode,
        IUserContext userContext,
        HttpContext httpContext,
        IApplicationReadDbContext readContext,
        FileAppLinkService linkService,
        FileStorageOptions storageOptions,
        CancellationToken cancellationToken)
    {
        return GetSecureLinkAsync(fileId, mode, userContext, httpContext, readContext, linkService, storageOptions, cancellationToken);
    }

    private static async Task<IResult> GetPublicByLinkAsync(
        string token,
        HttpContext httpContext,
        IApplicationReadDbContext readContext,
        IFileObjectStorage storage,
        FileAppLinkService linkService,
        CancellationToken cancellationToken)
    {
        string mode = ResolveModeFromRequest(httpContext.Request.Query["mode"]);
        if (!linkService.TryValidateToken(token, mode, DateTime.UtcNow, out Guid fileId))
        {
            return Results.Unauthorized();
        }

        FileAsset? file = await readContext.FileAssets
            .SingleOrDefaultAsync(x => x.Id == fileId && !x.IsDeleted, cancellationToken);

        if (file is null)
        {
            return Results.NotFound();
        }

        (Stream content, string contentType) = await storage.OpenReadAsync(file.ObjectKey, cancellationToken);

        if (string.Equals(mode, LinkModeDownload, StringComparison.Ordinal))
        {
            return Results.File(content, contentType, file.FileName, enableRangeProcessing: true);
        }

        httpContext.Response.Headers[HeaderNames.ContentDisposition] = $"inline; filename=\"{file.FileName}\"";
        return Results.File(content, contentType, fileDownloadName: null, enableRangeProcessing: true);
    }

    private static async Task<IResult> GetAuditAsync(Guid fileId, IUserContext userContext, IApplicationReadDbContext readContext, CancellationToken cancellationToken)
    {
        FileAsset? file = await readContext.FileAssets.SingleOrDefaultAsync(x => x.Id == fileId && !x.IsDeleted, cancellationToken);
        if (file is null)
        {
            return Results.NotFound();
        }

        if (!await HasReadAccessAsync(file, userContext.UserId, readContext, cancellationToken))
        {
            return Results.Forbid();
        }

        List<object> items = await readContext.FileAccessAudits
            .Where(x => x.FileId == fileId)
            .OrderByDescending(x => x.TimestampUtc)
            .Take(500)
            .Select(x => new { x.Id, x.Action, x.UserId, x.TimestampUtc, x.IpAddress })
            .Cast<object>()
            .ToListAsync(cancellationToken);

        return Results.Ok(new { fileId, total = items.Count, items });
    }

    private static async Task<IResult> DeleteAsync(Guid fileId, IUserContext userContext, HttpContext httpContext, IApplicationDbContext writeContext, IApplicationReadDbContext readContext, IFileObjectStorage storage, CancellationToken cancellationToken)
    {
        FileAsset? file = await writeContext.FileAssets.SingleOrDefaultAsync(x => x.Id == fileId && !x.IsDeleted, cancellationToken);
        if (file is null)
        {
            return Results.NotFound();
        }

        if (!await HasDeleteAccessAsync(file, userContext.UserId, readContext, cancellationToken))
        {
            return Results.Forbid();
        }

        await storage.DeleteAsync(file.ObjectKey, cancellationToken);
        file.IsDeleted = true;
        file.DeletedAtUtc = DateTime.UtcNow;
        file.UpdatedAtUtc = DateTime.UtcNow;
        await WriteAccessAuditAsync(writeContext, httpContext, file.Id, userContext.UserId, "delete", cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> UpdateMetadataAsync(Guid fileId, MetadataRequest request, IUserContext userContext, IApplicationDbContext writeContext, IApplicationReadDbContext readContext, CancellationToken cancellationToken)
    {
        FileAsset? file = await writeContext.FileAssets.SingleOrDefaultAsync(x => x.Id == fileId && !x.IsDeleted, cancellationToken);
        if (file is null)
        {
            return Results.NotFound();
        }

        if (!await HasWriteAccessAsync(file, userContext.UserId, readContext, cancellationToken))
        {
            return Results.Forbid();
        }

        if (!string.IsNullOrWhiteSpace(request.FileName))
        {
            file.FileName = SanitizeFileName(request.FileName);
        }

        file.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        file.UpdatedAtUtc = DateTime.UtcNow;
        await writeContext.SaveChangesAsync(cancellationToken);
        return Results.Ok(new { file.Id, file.FileName, file.Description, file.UpdatedAtUtc });
    }

    private static async Task<IResult> MoveAsync(Guid fileId, MoveRequest request, IUserContext userContext, IApplicationDbContext writeContext, IApplicationReadDbContext readContext, CancellationToken cancellationToken)
    {
        FileAsset? file = await writeContext.FileAssets.SingleOrDefaultAsync(x => x.Id == fileId && !x.IsDeleted, cancellationToken);
        if (file is null)
        {
            return Results.NotFound();
        }

        if (!await HasWriteAccessAsync(file, userContext.UserId, readContext, cancellationToken))
        {
            return Results.Forbid();
        }

        file.Module = string.IsNullOrWhiteSpace(request.Module) ? file.Module : request.Module.Trim();
        file.Folder = string.IsNullOrWhiteSpace(request.Folder) ? null : request.Folder.Trim();
        file.UpdatedAtUtc = DateTime.UtcNow;
        await writeContext.SaveChangesAsync(cancellationToken);
        return Results.Ok(new { file.Id, file.Module, file.Folder });
    }

    private static async Task<IResult> SearchAsync([AsParameters] SearchFilesRequest request, IUserContext userContext, IApplicationReadDbContext readContext, CancellationToken cancellationToken)
    {
        IQueryable<FileAsset> q = readContext.FileAssets.Where(x => !x.IsDeleted);
        bool isAdmin = await IsAdminAsync(userContext.UserId, readContext, cancellationToken);

        if (!isAdmin)
        {
            Guid uid = userContext.UserId;
            string uidText = uid.ToString("N");
            IQueryable<Guid> aclRead = readContext.FilePermissionEntries
                .Where(x => x.SubjectType == SubjectTypeUser && x.SubjectValue == uidText && x.CanRead)
                .Select(x => x.FileId);
            q = q.Where(x => x.OwnerUserId == uid || aclRead.Contains(x.Id));
        }

        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            string text = request.Query.Trim();
            q = q.Where(x => x.FileName.Contains(text) || x.Description != null && x.Description.Contains(text));
        }

        if (!string.IsNullOrWhiteSpace(request.FileType))
        {
            string ext = request.FileType.StartsWith('.') ? request.FileType.ToUpperInvariant() : $".{request.FileType.ToUpperInvariant()}";
            q = q.Where(x => x.Extension == ext);
        }

        if (request.UploaderId.HasValue)
        {
            q = q.Where(x => x.OwnerUserId == request.UploaderId.Value);
        }

        if (request.From.HasValue)
        {
            q = q.Where(x => x.UploadedAtUtc >= request.From.Value);
        }

        if (request.To.HasValue)
        {
            q = q.Where(x => x.UploadedAtUtc <= request.To.Value);
        }

        (int normalizedPage, int normalizedPageSize) = request.NormalizePaging();
        int total = await q.CountAsync(cancellationToken);
        List<object> items = await q.OrderByDescending(x => x.UploadedAtUtc)
            .Skip((normalizedPage - 1) * normalizedPageSize)
            .Take(normalizedPageSize)
            .Select(x => new { x.Id, x.FileName, x.Extension, x.ContentType, x.SizeBytes, x.Module, x.Folder, x.OwnerUserId, x.UploadedAtUtc })
            .Cast<object>()
            .ToListAsync(cancellationToken);
        return Results.Ok(new { page = normalizedPage, pageSize = normalizedPageSize, total, items });
    }

    private static async Task<IResult> FilterAsync([AsParameters] FilterFilesRequest request, IUserContext userContext, IApplicationReadDbContext readContext, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Module))
        {
            return Results.BadRequest(new { message = "module is required." });
        }

        IQueryable<FileAsset> q = readContext.FileAssets
            .Where(x => !x.IsDeleted && x.Module == request.Module);

        bool isAdmin = await IsAdminAsync(userContext.UserId, readContext, cancellationToken);
        if (!isAdmin)
        {
            Guid uid = userContext.UserId;
            string uidText = uid.ToString("N");
            IQueryable<Guid> aclRead = readContext.FilePermissionEntries
                .Where(x => x.SubjectType == SubjectTypeUser && x.SubjectValue == uidText && x.CanRead)
                .Select(x => x.FileId);

            q = q.Where(x => x.OwnerUserId == uid || aclRead.Contains(x.Id));
        }

        (int normalizedPage, int normalizedPageSize) = request.NormalizePaging();
        int total = await q.CountAsync(cancellationToken);
        List<object> items = await q.OrderByDescending(x => x.UploadedAtUtc)
            .Skip((normalizedPage - 1) * normalizedPageSize)
            .Take(normalizedPageSize)
            .Select(x => new { x.Id, x.FileName, x.ContentType, x.SizeBytes, x.Folder, x.UploadedAtUtc })
            .Cast<object>()
            .ToListAsync(cancellationToken);

        return Results.Ok(new { page = normalizedPage, pageSize = normalizedPageSize, total, items });
    }

    private static async Task<IResult> AddTagAsync(Guid fileId, TagRequest request, IUserContext userContext, IApplicationDbContext writeContext, IApplicationReadDbContext readContext, CancellationToken cancellationToken)
    {
        FileAsset? file = await writeContext.FileAssets.SingleOrDefaultAsync(x => x.Id == fileId && !x.IsDeleted, cancellationToken);
        if (file is null)
        {
            return Results.NotFound();
        }

        if (!await HasWriteAccessAsync(file, userContext.UserId, readContext, cancellationToken))
        {
            return Results.Forbid();
        }

        string tag = (request.Tag ?? string.Empty).Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(tag) || tag.Length > 80)
        {
            return Results.BadRequest(new { message = "Invalid tag." });
        }

        bool exists = await writeContext.FileTags.AnyAsync(x => x.FileId == fileId && x.Tag == tag, cancellationToken);
        if (!exists)
        {
            writeContext.FileTags.Add(new FileTag { FileId = fileId, Tag = tag });
            await writeContext.SaveChangesAsync(cancellationToken);
        }

        return Results.Ok(new { fileId, tag });
    }

    private static async Task<IResult> SearchByTagAsync(string tag, IUserContext userContext, IApplicationReadDbContext readContext, CancellationToken cancellationToken)
    {
        string normalized = tag.Trim().ToUpperInvariant();
        Guid uid = userContext.UserId;
        bool isAdmin = await IsAdminAsync(uid, readContext, cancellationToken);
        string uidText = uid.ToString("N");

        IQueryable<Guid> aclRead = readContext.FilePermissionEntries
            .Where(x => x.SubjectType == SubjectTypeUser && x.SubjectValue == uidText && x.CanRead)
            .Select(x => x.FileId);

        List<object> items = await (
            from file in readContext.FileAssets
            join ft in readContext.FileTags on file.Id equals ft.FileId
            where !file.IsDeleted && ft.Tag == normalized && (isAdmin || file.OwnerUserId == uid || aclRead.Contains(file.Id))
            orderby file.UploadedAtUtc descending
            select new { file.Id, file.FileName, file.ContentType, file.SizeBytes, file.Module, file.UploadedAtUtc })
            .Cast<object>()
            .ToListAsync(cancellationToken);

        return Results.Ok(new { total = items.Count, items });
    }

    private static async Task<IResult> UpsertPermissionAsync(Guid fileId, PermissionRequest request, IUserContext userContext, IApplicationDbContext writeContext, IApplicationReadDbContext readContext, CancellationToken cancellationToken)
    {
        FileAsset? file = await writeContext.FileAssets.SingleOrDefaultAsync(x => x.Id == fileId && !x.IsDeleted, cancellationToken);
        if (file is null)
        {
            return Results.NotFound();
        }

        bool ownerOrAdmin = file.OwnerUserId == userContext.UserId || await IsAdminAsync(userContext.UserId, readContext, cancellationToken);
        if (!ownerOrAdmin)
        {
            return Results.Forbid();
        }

        string subjectType = NormalizeSubjectType(request.SubjectType);
        if (subjectType.Length == 0)
        {
            return Results.BadRequest(new { message = "SubjectType must be User or Role." });
        }

        string subjectValue = (request.SubjectValue ?? string.Empty).Trim();
        if (subjectType == SubjectTypeUser && Guid.TryParse(subjectValue, out Guid parsedUser))
        {
            subjectValue = parsedUser.ToString("N");
        }

        FilePermissionEntry? existing = await writeContext.FilePermissionEntries
            .SingleOrDefaultAsync(x => x.FileId == fileId && x.SubjectType == subjectType && x.SubjectValue == subjectValue, cancellationToken);

        if (existing is null)
        {
            existing = new FilePermissionEntry
            {
                Id = Guid.NewGuid(),
                FileId = fileId,
                SubjectType = subjectType,
                SubjectValue = subjectValue,
                CreatedAtUtc = DateTime.UtcNow
            };
            writeContext.FilePermissionEntries.Add(existing);
        }

        existing.CanRead = request.CanRead;
        existing.CanWrite = request.CanWrite;
        existing.CanDelete = request.CanDelete;

        await writeContext.SaveChangesAsync(cancellationToken);
        return Results.Ok(new { existing.Id, existing.SubjectType, existing.SubjectValue, existing.CanRead, existing.CanWrite, existing.CanDelete });
    }

    private static async Task<IResult> GetPermissionsAsync(Guid fileId, IUserContext userContext, IApplicationReadDbContext readContext, CancellationToken cancellationToken)
    {
        FileAsset? file = await readContext.FileAssets.SingleOrDefaultAsync(x => x.Id == fileId && !x.IsDeleted, cancellationToken);
        if (file is null)
        {
            return Results.NotFound();
        }

        bool ownerOrAdmin = file.OwnerUserId == userContext.UserId || await IsAdminAsync(userContext.UserId, readContext, cancellationToken);
        if (!ownerOrAdmin)
        {
            return Results.Forbid();
        }

        List<object> items = await readContext.FilePermissionEntries
            .Where(x => x.FileId == fileId)
            .OrderBy(x => x.SubjectType)
            .ThenBy(x => x.SubjectValue)
            .Select(x => new { x.Id, x.SubjectType, x.SubjectValue, x.CanRead, x.CanWrite, x.CanDelete })
            .Cast<object>()
            .ToListAsync(cancellationToken);

        return Results.Ok(new { fileId, items });
    }

    private static Task<IResult> SetEncryptedAsync(Guid fileId, IUserContext userContext, IApplicationDbContext writeContext, IApplicationReadDbContext readContext, CancellationToken cancellationToken)
    {
        return SetEncryptionStateAsync(fileId, true, userContext, writeContext, readContext, cancellationToken);
    }

    private static Task<IResult> SetDecryptedAsync(Guid fileId, IUserContext userContext, IApplicationDbContext writeContext, IApplicationReadDbContext readContext, CancellationToken cancellationToken)
    {
        return SetEncryptionStateAsync(fileId, false, userContext, writeContext, readContext, cancellationToken);
    }

    private static async Task<IResult> SetEncryptionStateAsync(Guid fileId, bool encrypted, IUserContext userContext, IApplicationDbContext writeContext, IApplicationReadDbContext readContext, CancellationToken cancellationToken)
    {
        FileAsset? file = await writeContext.FileAssets.SingleOrDefaultAsync(x => x.Id == fileId && !x.IsDeleted, cancellationToken);
        if (file is null)
        {
            return Results.NotFound();
        }

        bool ownerOrAdmin = file.OwnerUserId == userContext.UserId || await IsAdminAsync(userContext.UserId, readContext, cancellationToken);
        if (!ownerOrAdmin)
        {
            return Results.Forbid();
        }

        file.IsEncrypted = encrypted;
        file.UpdatedAtUtc = DateTime.UtcNow;
        await writeContext.SaveChangesAsync(cancellationToken);
        return Results.Ok(new { fileId, encrypted });
    }

    private static async Task<bool> HasReadAccessAsync(FileAsset file, Guid userId, IApplicationReadDbContext readContext, CancellationToken cancellationToken)
    {
        if (file.OwnerUserId == userId || await IsAdminAsync(userId, readContext, cancellationToken))
        {
            return true;
        }

        string uid = userId.ToString("N");
        return await readContext.FilePermissionEntries.AnyAsync(
            x => x.FileId == file.Id &&
                 (x.SubjectType == SubjectTypeUser && x.SubjectValue == uid ||
                  x.SubjectType == SubjectTypeRole && UserRoleNames(readContext, userId).Contains(x.SubjectValue)) &&
                 x.CanRead,
            cancellationToken);
    }

    private static async Task<bool> HasWriteAccessAsync(FileAsset file, Guid userId, IApplicationReadDbContext readContext, CancellationToken cancellationToken)
    {
        if (file.OwnerUserId == userId || await IsAdminAsync(userId, readContext, cancellationToken))
        {
            return true;
        }

        string uid = userId.ToString("N");
        return await readContext.FilePermissionEntries.AnyAsync(
            x => x.FileId == file.Id &&
                 (x.SubjectType == SubjectTypeUser && x.SubjectValue == uid ||
                  x.SubjectType == SubjectTypeRole && UserRoleNames(readContext, userId).Contains(x.SubjectValue)) &&
                 x.CanWrite,
            cancellationToken);
    }

    private static async Task<bool> HasDeleteAccessAsync(FileAsset file, Guid userId, IApplicationReadDbContext readContext, CancellationToken cancellationToken)
    {
        if (file.OwnerUserId == userId || await IsAdminAsync(userId, readContext, cancellationToken))
        {
            return true;
        }

        string uid = userId.ToString("N");
        return await readContext.FilePermissionEntries.AnyAsync(
            x => x.FileId == file.Id &&
                 (x.SubjectType == SubjectTypeUser && x.SubjectValue == uid ||
                  x.SubjectType == SubjectTypeRole && UserRoleNames(readContext, userId).Contains(x.SubjectValue)) &&
                 x.CanDelete,
            cancellationToken);
    }

    private static IQueryable<string> UserRoleNames(IApplicationReadDbContext readContext, Guid userId)
    {
        return from ur in readContext.UserRoles
               join role in readContext.Roles on ur.RoleId equals role.Id
               where ur.UserId == userId
               select role.Name;
    }

    private static async Task<bool> IsAdminAsync(Guid userId, IApplicationReadDbContext readContext, CancellationToken cancellationToken)
    {
        return await (from ur in readContext.UserRoles
                      join role in readContext.Roles on ur.RoleId equals role.Id
                      where ur.UserId == userId && role.Name == "admin"
                      select ur.UserId).AnyAsync(cancellationToken);
    }

    private static async Task WriteAccessAuditAsync(IApplicationDbContext writeContext, HttpContext httpContext, Guid fileId, Guid userId, string action, CancellationToken cancellationToken)
    {
        writeContext.FileAccessAudits.Add(new FileAccessAudit
        {
            Id = Guid.NewGuid(),
            FileId = fileId,
            UserId = userId,
            Action = action,
            TimestampUtc = DateTime.UtcNow,
            IpAddress = httpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = httpContext.Request.Headers.UserAgent.ToString()
        });
        await writeContext.SaveChangesAsync(cancellationToken);
    }

    private static string BuildObjectKey(string prefix, string module, string extension)
    {
        string safeModule = string.IsNullOrWhiteSpace(module) ? "general" : module.Trim();
        return $"{prefix.Trim('/')}/{safeModule}/{DateTime.UtcNow:yyyy/MM/dd}/{Guid.NewGuid():N}{extension}";
    }

    private static string SanitizeFileName(string fileName)
    {
        string name = Path.GetFileName(fileName ?? string.Empty).Trim();
        if (name.Contains("..", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Path traversal detected.");
        }

        foreach (char invalid in Path.GetInvalidFileNameChars())
        {
            name = name.Replace(invalid, '_');
        }

        return string.IsNullOrWhiteSpace(name) ? $"file-{Guid.NewGuid():N}" : name;
    }

    private static (bool IsValid, string Message) ValidateFile(string fileName, long sizeBytes, string? contentType, FileValidationOptions options)
    {
        string safeName = Path.GetFileName(fileName ?? string.Empty);
        if (string.IsNullOrWhiteSpace(safeName))
        {
            return (false, "FileName is required.");
        }

        if (safeName.Contains("..", StringComparison.Ordinal))
        {
            return (false, "Invalid file name.");
        }

        long maxBytes = Math.Max(1, options.MaxFileSizeMb) * 1024L * 1024L;
        if (sizeBytes <= 0 || sizeBytes > maxBytes)
        {
            return (false, $"File size exceeds limit ({options.MaxFileSizeMb}MB).");
        }

        string ext = Path.GetExtension(safeName).ToUpperInvariant();
        if (!options.AllowedExtensions.Any(x => string.Equals(x, ext, StringComparison.OrdinalIgnoreCase)))
        {
            return (false, "File extension is not allowed.");
        }

        if (string.IsNullOrWhiteSpace(contentType))
        {
            return (false, "Content type is required.");
        }

        return (true, "Valid file.");
    }

    private static string ComputeSha256(Stream stream)
    {
        stream.Position = 0;
        byte[] hash = SHA256.HashData(stream);
        stream.Position = 0;
        return Convert.ToHexString(hash);
    }

    private static async Task<bool> HasOwnerOrAdminAccessAsync(
        FileAsset file,
        Guid userId,
        IApplicationReadDbContext readContext,
        CancellationToken cancellationToken)
    {
        return file.OwnerUserId == userId || await IsAdminAsync(userId, readContext, cancellationToken);
    }

    private static string NormalizeLinkMode(string? mode)
    {
        if (string.Equals(mode, LinkModeDownload, StringComparison.OrdinalIgnoreCase))
        {
            return LinkModeDownload;
        }

        if (string.Equals(mode, LinkModeStream, StringComparison.OrdinalIgnoreCase))
        {
            return LinkModeStream;
        }

        return LinkModeInline;
    }

    private static string ResolveModeFromRequest(string? mode)
    {
        return NormalizeLinkMode(mode);
    }

    private static string BuildAppLinkUrl(HttpContext httpContext, string token, string mode)
    {
        return $"{httpContext.Request.Scheme}://{httpContext.Request.Host}/api/v1/files/public/{token}?mode={mode}";
    }

    private static string NormalizeSubjectType(string subjectType)
    {
        if (string.Equals(subjectType, SubjectTypeUser, StringComparison.OrdinalIgnoreCase))
        {
            return SubjectTypeUser;
        }

        if (string.Equals(subjectType, SubjectTypeRole, StringComparison.OrdinalIgnoreCase))
        {
            return SubjectTypeRole;
        }

        return string.Empty;
    }
}
