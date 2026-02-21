# ماژول Files

تاریخ به‌روزرسانی: 2026-02-21

## هدف
ارائه سرویس مدیریت فایل سازمانی با امنیت، ACL، لینک عمومی امن و audit دسترسی.

## مسئولیت‌های اصلی
- آپلود و اعتبارسنجی/اسکن فایل
- نگهداری metadata، دانلود و stream
- اشتراک‌گذاری از طریق secure/public link
- مدیریت تگ، جستجو و فیلتر
- مدیریت ACL و مجوزهای فایل
- مدیریت وضعیت رمزنگاری فایل

## مدل دامنه
- `FileAsset`
- `FileTag`
- `FilePermissionEntry`
- `FileAccessAudit`

## Use caseهای کلیدی
- `UploadFileCommand`, `ValidateFileCommand`, `ScanFileCommand`
- `GetFileMetadataQuery`, `DownloadFileQuery`, `StreamFileQuery`
- `GetSecureFileLinkQuery`, `GetPublicFileByLinkQuery`
- `SearchFilesQuery`, `FilterFilesQuery`, `SearchFilesByTagQuery`
- `UpsertFilePermissionCommand`, `GetFilePermissionsQuery`

## API و سطح دسترسی
- مسیرها: `/api/v1/files/*`
- `files.read`, `files.write`, `files.delete`, `files.share`, `files.permissions.manage`
- مسیر عمومی: `/api/v1/files/public/{token}` بدون JWT ولی با token contextual

## وابستگی‌ها
- MinIO/Object Storage
- ClamAV (اختیاری) برای اسکن بدافزار
- Logging برای audit لینک‌های عمومی

## داده و نگهداشت
- اسکیما: `files`
- مسیرهای public باید rate limit و expiry داشته باشند.

## نکات عملیاتی
- کلید signing لینک عمومی باید در secret manager نگهداری شود.
- اسکن بدافزار در production باید فعال و monitor شود.

## ریسک‌ها
- نشتی token لینک عمومی یا expiry نادرست می‌تواند منجر به افشای فایل شود.
