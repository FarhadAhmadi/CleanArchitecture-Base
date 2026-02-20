using System.Security.Cryptography;
using System.Text;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Files;
using Domain.Files;
using Domain.Logging;
using Infrastructure.Files;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;

using Application.Modules.Files;

namespace Web.Api.Endpoints.Files;

internal sealed class FileUseCaseService(
    IUserContext userContext,
    IApplicationDbContext writeContext,
    IApplicationReadDbContext readContext,
    IFileObjectStorage storage,
    IFileMalwareScanner scanner,
    FileValidationOptions validationOptions,
    FileStorageOptions storageOptions,
    FileAppLinkService linkService) : IFileUseCaseService
{
    private const string SubjectTypeUser = "User";
    private const string SubjectTypeRole = "Role";
    private const string LinkModeInline = "inline";
    private const string LinkModeDownload = "download";
    private const string LinkModeStream = "stream";

    public async Task<IResult> UploadAsync(UploadFileInput input, HttpContext httpContext, CancellationToken cancellationToken)
    {
        if (input.File.Length == 0)
        {
            return Results.BadRequest(new { message = "Empty file is not allowed." });
        }

        (bool isValid, string validationMessage) = ValidateFile(input.File.FileName, input.File.Length, input.File.ContentType, validationOptions);
        if (!isValid)
        {
            return Results.BadRequest(new { message = validationMessage });
        }

        string safeFileName = SanitizeFileName(input.File.FileName);
        string extension = Path.GetExtension(safeFileName).ToUpperInvariant();
        string objectKey = BuildObjectKey(storageOptions.ObjectPrefix, input.Module, extension);

        await using Stream source = input.File.OpenReadStream();
        await using var copy = new MemoryStream((int)input.File.Length);
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

        var entity = new FileAsset
        {
            Id = Guid.NewGuid(),
            OwnerUserId = userContext.UserId,
            ObjectKey = objectKey,
            FileName = safeFileName,
            Extension = extension,
            ContentType = input.File.ContentType ?? "application/octet-stream",
            SizeBytes = input.File.Length,
            Module = string.IsNullOrWhiteSpace(input.Module) ? "General" : input.Module.Trim(),
            Folder = string.IsNullOrWhiteSpace(input.Folder) ? null : input.Folder.Trim(),
            Description = string.IsNullOrWhiteSpace(input.Description) ? null : input.Description.Trim(),
            Sha256 = sha256,
            IsScanned = true,
            UploadedAtUtc = DateTime.UtcNow
        };

        bool uploadCompleted = false;
        try
        {
            await storage.UploadAsync(objectKey, copy, copy.Length, input.File.ContentType ?? "application/octet-stream", cancellationToken);
            uploadCompleted = true;

            writeContext.FileAssets.Add(entity);
            await WriteAccessAuditAsync(httpContext, entity.Id, userContext.UserId, "upload", cancellationToken);
            await writeContext.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            if (uploadCompleted)
            {
                try
                {
                    await storage.DeleteAsync(objectKey, cancellationToken);
                }
                catch
                {
                    // Best effort compensation to avoid orphan objects when DB write fails.
                }
            }

            throw;
        }

        return Results.Ok(new { fileId = entity.Id, entity.FileName, entity.SizeBytes, entity.ContentType, entity.Module, entity.UploadedAtUtc });
    }

    public IResult Validate(ValidateFileInput input)
    {
        (bool isValid, string message) = ValidateFile(input.FileName, input.SizeBytes, input.ContentType, validationOptions);
        return Results.Ok(new { isValid, message });
    }

    public async Task<IResult> ScanAsync(ScanFileInput input, CancellationToken cancellationToken)
    {
        await using Stream stream = input.File.OpenReadStream();
        FileScanResult result = await scanner.ScanAsync(stream, cancellationToken);
        return Results.Ok(new { result.IsClean, result.Message });
    }

    public async Task<IResult> GetMetadataAsync(Guid fileId, CancellationToken cancellationToken)
    {
        FileAsset? file = await readContext.FileAssets.SingleOrDefaultAsync(x => x.Id == fileId && !x.IsDeleted, cancellationToken);
        if (file is null)
        {
            return Results.NotFound();
        }

        if (!await HasReadAccessAsync(file, userContext.UserId, cancellationToken))
        {
            return Results.Forbid();
        }

        return Results.Ok(new { file.Id, file.FileName, file.ContentType, file.SizeBytes, file.Module, file.Folder, file.Description, file.OwnerUserId, file.UploadedAtUtc, file.UpdatedAtUtc, file.IsEncrypted });
    }

    public async Task<IResult> DownloadAsync(Guid fileId, HttpContext httpContext, CancellationToken cancellationToken)
    {
        FileAsset? file = await readContext.FileAssets.SingleOrDefaultAsync(x => x.Id == fileId && !x.IsDeleted, cancellationToken);
        if (file is null)
        {
            return Results.NotFound();
        }

        if (!await HasReadAccessAsync(file, userContext.UserId, cancellationToken))
        {
            return Results.Forbid();
        }

        (Stream content, string contentType) = await storage.OpenReadAsync(file.ObjectKey, cancellationToken);
        await WriteAccessAuditAsync(httpContext, file.Id, userContext.UserId, "download", cancellationToken);
        await writeContext.SaveChangesAsync(cancellationToken);

        return Results.File(
            content,
            contentType,
            fileDownloadName: file.FileName,
            enableRangeProcessing: true);
    }

    public async Task<IResult> StreamAsync(Guid fileId, HttpContext httpContext, CancellationToken cancellationToken)
    {
        FileAsset? file = await readContext.FileAssets.SingleOrDefaultAsync(x => x.Id == fileId && !x.IsDeleted, cancellationToken);
        if (file is null)
        {
            return Results.NotFound();
        }

        if (!await HasReadAccessAsync(file, userContext.UserId, cancellationToken))
        {
            return Results.Forbid();
        }

        (Stream content, string contentType) = await storage.OpenReadAsync(file.ObjectKey, cancellationToken);
        await WriteAccessAuditAsync(httpContext, file.Id, userContext.UserId, "stream", cancellationToken);
        await writeContext.SaveChangesAsync(cancellationToken);

        httpContext.Response.Headers[HeaderNames.ContentDisposition] = $"inline; filename=\"{file.FileName}\"";

        return Results.File(
            content,
            contentType,
            fileDownloadName: null,
            enableRangeProcessing: true);
    }

    public async Task<IResult> GetSecureLinkAsync(Guid fileId, string? mode, HttpContext httpContext, CancellationToken cancellationToken)
    {
        FileAsset? file = await readContext.FileAssets.SingleOrDefaultAsync(x => x.Id == fileId && !x.IsDeleted, cancellationToken);
        if (file is null)
        {
            return Results.NotFound();
        }

        if (!await HasReadAccessAsync(file, userContext.UserId, cancellationToken))
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

    public Task<IResult> ShareAsync(Guid fileId, string? mode, HttpContext httpContext, CancellationToken cancellationToken)
    {
        return GetSecureLinkAsync(fileId, mode, httpContext, cancellationToken);
    }

    public async Task<IResult> GetPublicByLinkAsync(string token, HttpContext httpContext, CancellationToken cancellationToken)
    {
        string mode = ResolveModeFromRequest(httpContext.Request.Query["mode"]);
        if (!linkService.TryValidateToken(token, mode, DateTime.UtcNow, out Guid fileId))
        {
            await WritePublicLinkAuditAsync(httpContext, token, mode, null, "invalid-token", cancellationToken);
            await writeContext.SaveChangesAsync(cancellationToken);
            return Results.Unauthorized();
        }

        FileAsset? file = await readContext.FileAssets
            .SingleOrDefaultAsync(x => x.Id == fileId && !x.IsDeleted, cancellationToken);

        if (file is null)
        {
            await WritePublicLinkAuditAsync(httpContext, token, mode, fileId, "file-not-found", cancellationToken);
            await writeContext.SaveChangesAsync(cancellationToken);
            return Results.NotFound();
        }

        (Stream content, string contentType) = await storage.OpenReadAsync(file.ObjectKey, cancellationToken);
        await WriteAccessAuditAsync(httpContext, file.Id, null, $"public-{mode}", cancellationToken);
        await WritePublicLinkAuditAsync(httpContext, token, mode, file.Id, "success", cancellationToken);
        await writeContext.SaveChangesAsync(cancellationToken);

        if (string.Equals(mode, LinkModeDownload, StringComparison.Ordinal))
        {
            return Results.File(content, contentType, file.FileName, enableRangeProcessing: true);
        }

        httpContext.Response.Headers[HeaderNames.ContentDisposition] = $"inline; filename=\"{file.FileName}\"";
        return Results.File(content, contentType, fileDownloadName: null, enableRangeProcessing: true);
    }

    public async Task<IResult> GetAuditAsync(Guid fileId, CancellationToken cancellationToken)
    {
        FileAsset? file = await readContext.FileAssets.SingleOrDefaultAsync(x => x.Id == fileId && !x.IsDeleted, cancellationToken);
        if (file is null)
        {
            return Results.NotFound();
        }

        if (!await HasReadAccessAsync(file, userContext.UserId, cancellationToken))
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

    public async Task<IResult> DeleteAsync(Guid fileId, HttpContext httpContext, CancellationToken cancellationToken)
    {
        FileAsset? file = await writeContext.FileAssets.SingleOrDefaultAsync(x => x.Id == fileId && !x.IsDeleted, cancellationToken);
        if (file is null)
        {
            return Results.NotFound();
        }

        if (!await HasDeleteAccessAsync(file, userContext.UserId, cancellationToken))
        {
            return Results.Forbid();
        }

        await storage.DeleteAsync(file.ObjectKey, cancellationToken);
        file.IsDeleted = true;
        file.DeletedAtUtc = DateTime.UtcNow;
        file.UpdatedAtUtc = DateTime.UtcNow;
        await WriteAccessAuditAsync(httpContext, file.Id, userContext.UserId, "delete", cancellationToken);
        await writeContext.SaveChangesAsync(cancellationToken);
        return Results.NoContent();
    }

    public async Task<IResult> UpdateMetadataAsync(Guid fileId, UpdateFileMetadataInput input, CancellationToken cancellationToken)
    {
        FileAsset? file = await writeContext.FileAssets.SingleOrDefaultAsync(x => x.Id == fileId && !x.IsDeleted, cancellationToken);
        if (file is null)
        {
            return Results.NotFound();
        }

        if (!await HasWriteAccessAsync(file, userContext.UserId, cancellationToken))
        {
            return Results.Forbid();
        }

        if (!string.IsNullOrWhiteSpace(input.FileName))
        {
            file.FileName = SanitizeFileName(input.FileName);
        }

        file.Description = string.IsNullOrWhiteSpace(input.Description) ? null : input.Description.Trim();
        file.UpdatedAtUtc = DateTime.UtcNow;
        await writeContext.SaveChangesAsync(cancellationToken);
        return Results.Ok(new { file.Id, file.FileName, file.Description, file.UpdatedAtUtc });
    }

    public async Task<IResult> MoveAsync(Guid fileId, MoveFileInput input, CancellationToken cancellationToken)
    {
        FileAsset? file = await writeContext.FileAssets.SingleOrDefaultAsync(x => x.Id == fileId && !x.IsDeleted, cancellationToken);
        if (file is null)
        {
            return Results.NotFound();
        }

        if (!await HasWriteAccessAsync(file, userContext.UserId, cancellationToken))
        {
            return Results.Forbid();
        }

        file.Module = string.IsNullOrWhiteSpace(input.Module) ? file.Module : input.Module.Trim();
        file.Folder = string.IsNullOrWhiteSpace(input.Folder) ? null : input.Folder.Trim();
        file.UpdatedAtUtc = DateTime.UtcNow;
        await writeContext.SaveChangesAsync(cancellationToken);
        return Results.Ok(new { file.Id, file.Module, file.Folder });
    }

    public async Task<IResult> SearchAsync(SearchFilesInput input, CancellationToken cancellationToken)
    {
        IQueryable<FileAsset> q = readContext.FileAssets.Where(x => !x.IsDeleted);

        bool isAdmin = await IsAdminAsync(userContext.UserId, cancellationToken);
        if (!isAdmin)
        {
            Guid uid = userContext.UserId;
            string uidText = uid.ToString("N");
            IQueryable<Guid> aclRead = readContext.FilePermissionEntries
                .Where(x => x.SubjectType == SubjectTypeUser && x.SubjectValue == uidText && x.CanRead)
                .Select(x => x.FileId);
            q = q.Where(x => x.OwnerUserId == uid || aclRead.Contains(x.Id));
        }

        if (!string.IsNullOrWhiteSpace(input.Query))
        {
            string text = input.Query.Trim();
            q = q.Where(x => x.FileName.Contains(text) || x.Description != null && x.Description.Contains(text));
        }

        if (!string.IsNullOrWhiteSpace(input.FileType))
        {
            string ext = input.FileType.StartsWith('.') ? input.FileType.ToUpperInvariant() : $".{input.FileType.ToUpperInvariant()}";
            q = q.Where(x => x.Extension == ext);
        }

        if (input.UploaderId.HasValue)
        {
            q = q.Where(x => x.OwnerUserId == input.UploaderId.Value);
        }

        if (input.From.HasValue)
        {
            q = q.Where(x => x.UploadedAtUtc >= input.From.Value);
        }

        if (input.To.HasValue)
        {
            q = q.Where(x => x.UploadedAtUtc <= input.To.Value);
        }

        int normalizedPage = Math.Max(1, input.Page);
        int normalizedPageSize = Math.Clamp(input.PageSize <= 0 ? 50 : input.PageSize, 1, 200);
        int total = await q.CountAsync(cancellationToken);
        List<object> items = await q.OrderByDescending(x => x.UploadedAtUtc)
            .Skip((normalizedPage - 1) * normalizedPageSize)
            .Take(normalizedPageSize)
            .Select(x => new { x.Id, x.FileName, x.Extension, x.ContentType, x.SizeBytes, x.Module, x.Folder, x.OwnerUserId, x.UploadedAtUtc })
            .Cast<object>()
            .ToListAsync(cancellationToken);
        return Results.Ok(new { page = normalizedPage, pageSize = normalizedPageSize, total, items });
    }

    public async Task<IResult> FilterAsync(FilterFilesInput input, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(input.Module))
        {
            return Results.BadRequest(new { message = "module is required." });
        }

        IQueryable<FileAsset> q = readContext.FileAssets
            .Where(x => !x.IsDeleted && x.Module == input.Module);

        bool isAdmin = await IsAdminAsync(userContext.UserId, cancellationToken);
        if (!isAdmin)
        {
            Guid uid = userContext.UserId;
            string uidText = uid.ToString("N");
            IQueryable<Guid> aclRead = readContext.FilePermissionEntries
                .Where(x => x.SubjectType == SubjectTypeUser && x.SubjectValue == uidText && x.CanRead)
                .Select(x => x.FileId);

            q = q.Where(x => x.OwnerUserId == uid || aclRead.Contains(x.Id));
        }

        int normalizedPage = Math.Max(1, input.Page);
        int normalizedPageSize = Math.Clamp(input.PageSize <= 0 ? 50 : input.PageSize, 1, 200);
        int total = await q.CountAsync(cancellationToken);
        List<object> items = await q.OrderByDescending(x => x.UploadedAtUtc)
            .Skip((normalizedPage - 1) * normalizedPageSize)
            .Take(normalizedPageSize)
            .Select(x => new { x.Id, x.FileName, x.ContentType, x.SizeBytes, x.Folder, x.UploadedAtUtc })
            .Cast<object>()
            .ToListAsync(cancellationToken);

        return Results.Ok(new { page = normalizedPage, pageSize = normalizedPageSize, total, items });
    }

    public async Task<IResult> AddTagAsync(Guid fileId, AddFileTagInput input, CancellationToken cancellationToken)
    {
        FileAsset? file = await writeContext.FileAssets.SingleOrDefaultAsync(x => x.Id == fileId && !x.IsDeleted, cancellationToken);
        if (file is null)
        {
            return Results.NotFound();
        }

        if (!await HasWriteAccessAsync(file, userContext.UserId, cancellationToken))
        {
            return Results.Forbid();
        }

        string tag = (input.Tag ?? string.Empty).Trim().ToUpperInvariant();
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

    public async Task<IResult> SearchByTagAsync(string tag, CancellationToken cancellationToken)
    {
        string normalized = tag.Trim().ToUpperInvariant();
        Guid uid = userContext.UserId;
        bool isAdmin = await IsAdminAsync(uid, cancellationToken);
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

    public async Task<IResult> UpsertPermissionAsync(Guid fileId, UpsertFilePermissionInput input, CancellationToken cancellationToken)
    {
        FileAsset? file = await writeContext.FileAssets.SingleOrDefaultAsync(x => x.Id == fileId && !x.IsDeleted, cancellationToken);
        if (file is null)
        {
            return Results.NotFound();
        }

        bool ownerOrAdmin = file.OwnerUserId == userContext.UserId || await IsAdminAsync(userContext.UserId, cancellationToken);
        if (!ownerOrAdmin)
        {
            return Results.Forbid();
        }

        string subjectType = NormalizeSubjectType(input.SubjectType);
        if (subjectType.Length == 0)
        {
            return Results.BadRequest(new { message = "SubjectType must be User or Role." });
        }

        string subjectValue = (input.SubjectValue ?? string.Empty).Trim();
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

        existing.CanRead = input.CanRead;
        existing.CanWrite = input.CanWrite;
        existing.CanDelete = input.CanDelete;

        await writeContext.SaveChangesAsync(cancellationToken);
        return Results.Ok(new { existing.Id, existing.SubjectType, existing.SubjectValue, existing.CanRead, existing.CanWrite, existing.CanDelete });
    }

    public async Task<IResult> GetPermissionsAsync(Guid fileId, CancellationToken cancellationToken)
    {
        FileAsset? file = await readContext.FileAssets.SingleOrDefaultAsync(x => x.Id == fileId && !x.IsDeleted, cancellationToken);
        if (file is null)
        {
            return Results.NotFound();
        }

        bool ownerOrAdmin = file.OwnerUserId == userContext.UserId || await IsAdminAsync(userContext.UserId, cancellationToken);
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

    public Task<IResult> SetEncryptedAsync(Guid fileId, CancellationToken cancellationToken)
    {
        return SetEncryptionStateAsync(fileId, true, cancellationToken);
    }

    public Task<IResult> SetDecryptedAsync(Guid fileId, CancellationToken cancellationToken)
    {
        return SetEncryptionStateAsync(fileId, false, cancellationToken);
    }

    private async Task<IResult> SetEncryptionStateAsync(Guid fileId, bool encrypted, CancellationToken cancellationToken)
    {
        FileAsset? file = await writeContext.FileAssets.SingleOrDefaultAsync(x => x.Id == fileId && !x.IsDeleted, cancellationToken);
        if (file is null)
        {
            return Results.NotFound();
        }

        bool ownerOrAdmin = file.OwnerUserId == userContext.UserId || await IsAdminAsync(userContext.UserId, cancellationToken);
        if (!ownerOrAdmin)
        {
            return Results.Forbid();
        }

        file.IsEncrypted = encrypted;
        file.UpdatedAtUtc = DateTime.UtcNow;
        await writeContext.SaveChangesAsync(cancellationToken);
        return Results.Ok(new { fileId, encrypted });
    }

    private async Task<bool> HasReadAccessAsync(FileAsset file, Guid userId, CancellationToken cancellationToken)
    {
        if (file.OwnerUserId == userId || await IsAdminAsync(userId, cancellationToken))
        {
            return true;
        }

        string uid = userId.ToString("N");
        return await readContext.FilePermissionEntries.AnyAsync(
            x => x.FileId == file.Id &&
                 (x.SubjectType == SubjectTypeUser && x.SubjectValue == uid ||
                  x.SubjectType == SubjectTypeRole && UserRoleNames(userId).Contains(x.SubjectValue)) &&
                 x.CanRead,
            cancellationToken);
    }

    private async Task<bool> HasWriteAccessAsync(FileAsset file, Guid userId, CancellationToken cancellationToken)
    {
        if (file.OwnerUserId == userId || await IsAdminAsync(userId, cancellationToken))
        {
            return true;
        }

        string uid = userId.ToString("N");
        return await readContext.FilePermissionEntries.AnyAsync(
            x => x.FileId == file.Id &&
                 (x.SubjectType == SubjectTypeUser && x.SubjectValue == uid ||
                  x.SubjectType == SubjectTypeRole && UserRoleNames(userId).Contains(x.SubjectValue)) &&
                 x.CanWrite,
            cancellationToken);
    }

    private async Task<bool> HasDeleteAccessAsync(FileAsset file, Guid userId, CancellationToken cancellationToken)
    {
        if (file.OwnerUserId == userId || await IsAdminAsync(userId, cancellationToken))
        {
            return true;
        }

        string uid = userId.ToString("N");
        return await readContext.FilePermissionEntries.AnyAsync(
            x => x.FileId == file.Id &&
                 (x.SubjectType == SubjectTypeUser && x.SubjectValue == uid ||
                  x.SubjectType == SubjectTypeRole && UserRoleNames(userId).Contains(x.SubjectValue)) &&
                 x.CanDelete,
            cancellationToken);
    }

    private IQueryable<string> UserRoleNames(Guid userId)
    {
        return from ur in readContext.UserRoles
               join role in readContext.Roles on ur.RoleId equals role.Id
               where ur.UserId == userId
               select role.Name;
    }

    private async Task<bool> IsAdminAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await (from ur in readContext.UserRoles
                      join role in readContext.Roles on ur.RoleId equals role.Id
                      where ur.UserId == userId && role.Name == "admin"
                      select ur.UserId).AnyAsync(cancellationToken);
    }

    private Task WriteAccessAuditAsync(HttpContext httpContext, Guid fileId, Guid? userId, string action, CancellationToken cancellationToken)
    {
        _ = cancellationToken;
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

        return Task.CompletedTask;
    }

    private Task WritePublicLinkAuditAsync(HttpContext httpContext, string token, string mode, Guid? fileId, string outcome, CancellationToken cancellationToken)
    {
        string tokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
        string checksumPayload = $"{httpContext.TraceIdentifier}|{tokenHash}|{mode}|{fileId?.ToString("N")}|{outcome}";
        string checksum = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(checksumPayload)));

        writeContext.LogEvents.Add(new LogEvent
        {
            Id = Guid.NewGuid(),
            TimestampUtc = DateTime.UtcNow,
            Level = LogLevelType.Audit,
            Message = "Public file link access attempt",
            SourceService = "Web.Api",
            SourceModule = "Files",
            TraceId = httpContext.TraceIdentifier,
            ActorType = "Anonymous",
            Outcome = outcome,
            Ip = httpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = httpContext.Request.Headers.UserAgent.ToString(),
            HttpMethod = httpContext.Request.Method,
            HttpPath = httpContext.Request.Path,
            HttpStatusCode = null,
            TagsCsv = "files,public-link,audit",
            PayloadJson = $"{{\"tokenHash\":\"{tokenHash}\",\"mode\":\"{mode}\",\"fileId\":\"{fileId?.ToString("N")}\"}}",
            Checksum = checksum
        });

        _ = cancellationToken;
        return Task.CompletedTask;
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



