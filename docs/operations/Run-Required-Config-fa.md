# کانفیگ‌های ضروری برای اجرای پروژه

این فایل فقط روی «چیزهایی که باید تنظیم کنی تا پروژه بالا بیاید» تمرکز دارد.

## سناریو 1: اجرای راحت با Docker (پیشنهادی)
اگر از دستور زیر استفاده می‌کنی:

```powershell
.\scripts\dev\dev-stack.ps1 up
```

برای بالا آمدن اولیه، عملاً کانفیگ اجباری اضافه نداری چون مقادیر اصلی در:
- `docker-compose.yml`
- `docker-compose.override.yml`
- `src/Web.Api/appsettings.Development.json`
از قبل تنظیم شده‌اند.

فقط این پیش‌نیازها را داشته باش:
- Docker Desktop
- .NET SDK

## سناریو 2: اجرای دستی API با `dotnet run`
اگر API را دستی اجرا می‌کنی (بدون `dev-stack.ps1`)، این‌ها حداقل تنظیمات اجباری هستند:

1. `ConnectionStrings:Database`
2. `Jwt:Secret`
3. `Jwt:Issuer`
4. `Jwt:Audience`
5. `Notifications:SensitiveDataEncryptionKey`

حداقل نمونه:

```json
{
  "ConnectionStrings": {
    "Database": "Server=localhost;Initial Catalog=CleanArchitectureBase;User Id=sa;Password=Your_strong_password123!;TrustServerCertificate=True;MultipleActiveResultSets=True;"
  },
  "Jwt": {
    "Secret": "replace-with-strong-secret-at-least-32-chars",
    "Issuer": "clean-architecture",
    "Audience": "developers"
  },
  "Notifications": {
    "Enabled": true,
    "SensitiveDataEncryptionKey": "replace-with-strong-key"
  }
}
```

## کانفیگ‌های ضروری هر ماژول

### 1) Core API
- `ConnectionStrings:Database` (اجباری)
- `ApiSecurity:*` (پیشنهادی، برای امنیت/ریت‌لیمیت)

### 2) Auth
- `Jwt:Secret` (اجباری)
- `Jwt:Issuer` (اجباری)
- `Jwt:Audience` (اجباری)
- `Jwt:ExpirationInMinutes` (پیشنهادی)

### 3) File Module (MinIO)
برای فعال بودن ماژول فایل:
- `FileStorage:Enabled=true`
- `FileStorage:Endpoint`
- `FileStorage:AccessKey`
- `FileStorage:SecretKey`
- `FileStorage:Bucket`
- `FileValidation:MaxFileSizeMb`
- `FileValidation:AllowedExtensions`

### 4) Notification Module (هسته)
برای کارکرد خود ماژول اعلان:
- `Notifications:Enabled=true`
- `Notifications:SensitiveDataEncryptionKey` (اجباری)
- `Notifications:MaxRetries`
- `Notifications:BaseRetryDelaySeconds`
- `Notifications:DispatchBatchSize`
- `Notifications:DispatchPollingSeconds`

## کانفیگ ارسال واقعی اعلان‌ها (اختیاری اما لازم برای Provider واقعی)
اگر می‌خواهی واقعاً پیام ارسال شود (نه فقط ثبت/dispatch داخلی)، باید provider هر کانال را تنظیم کنی:

### Email
- `Notifications:Email:Enabled=true`
- `Notifications:Email:Host`
- `Notifications:Email:Port`
- `Notifications:Email:UseSsl`
- `Notifications:Email:UserName`
- `Notifications:Email:Password`
- `Notifications:Email:FromAddress`

### SMS
- `Notifications:Sms:Enabled=true`
- `Notifications:Sms:BaseUrl`
- `Notifications:Sms:EndpointPath`
- `Notifications:Sms:ApiKey`
- `Notifications:Sms:ApiKeyHeaderName` یا `UseBearerToken=true`
- `Notifications:Sms:SenderId`

### Slack
- `Notifications:Slack:Enabled=true`
- یکی از این دو:
1. `Notifications:Slack:WebhookUrl`
2. `Notifications:Slack:BotToken` + `Notifications:Slack:PostMessageApiUrl`

### Teams
- `Notifications:Teams:Enabled=true`
- `Notifications:Teams:WebhookUrl`

### Push
- `Notifications:Push:Enabled=true`
- `Notifications:Push:BaseUrl`
- `Notifications:Push:EndpointPath`
- `Notifications:Push:ApiKey`

### InApp
- `Notifications:InApp:Enabled=true`

## سرویس‌های زیرساختی (بسته به نیاز)
- RabbitMQ:
  - `RabbitMq:Enabled=true`
  - `RabbitMq:Host`, `Port`, `UserName`, `Password`
- Redis:
  - `RedisCache:Enabled=true`
  - `RedisCache:ConnectionString`
- Log/Observability (اختیاری برای اجرا، مهم برای عملیات):
  - `Serilog:*`
  - `OperationalAlerting:*`

## چک‌لیست سریع قبل از Run
1. `ConnectionStrings:Database` درست است.
2. `Jwt` کامل و معتبر است.
3. `Notifications:SensitiveDataEncryptionKey` مقدار امن دارد.
4. اگر Email/SMS/Slack می‌خواهی، `Enabled=true` + credential/provider کامل است.
5. اگر File لازم داری، `FileStorage` و MinIO تنظیم است.
