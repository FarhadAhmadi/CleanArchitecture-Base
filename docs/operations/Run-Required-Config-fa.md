# کانفیگ‌های ضروری برای اجرای پروژه

## 1) اجرای Docker (پیشنهادی)
اگر از `scripts/dev/dev-stack.ps1` استفاده می‌کنی، برای بالا آمدن اولیه معمولاً کانفیگ اضافه لازم نیست چون مقادیر پایه در `appsettings.Development.json` و `docker-compose*.yml` وجود دارد.

## 2) حداقل کانفیگ برای اجرای دستی API

### ضروری
1. `ConnectionStrings:Database`
2. `Jwt:Secret`
3. `Jwt:Issuer`
4. `Jwt:Audience`
5. `Notifications:SensitiveDataEncryptionKey`

### نمونه حداقلی
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

## 3) کانفیگ ماژول Files
1. `FileStorage:Enabled`
2. `FileStorage:Endpoint`
3. `FileStorage:AccessKey`
4. `FileStorage:SecretKey`
5. `FileStorage:Bucket`

## 4) کانفیگ ماژول Notifications

### هسته ماژول
1. `Notifications:Enabled`
2. `Notifications:MaxRetries`
3. `Notifications:DispatchBatchSize`
4. `Notifications:DispatchPollingSeconds`
5. `Notifications:SensitiveDataEncryptionKey`

### providerها (برای ارسال واقعی)
1. Email: `Notifications:Email:*`
2. SMS: `Notifications:Sms:*`
3. Slack: `Notifications:Slack:*`
4. Teams: `Notifications:Teams:*`
5. Push: `Notifications:Push:*`
6. InApp: `Notifications:InApp:Enabled`

### راه‌اندازی سریع SMTP برای Email
به‌جای ذخیره credential در `appsettings` از `user-secrets` استفاده کن:

```powershell
.\scripts\dev\configure-email.ps1 `
  -SmtpHost "smtp.gmail.com" `
  -Port 587 `
  -UseSsl $true `
  -UserName "farhadahmadi14132@gmail.com" `
  -Password "<GMAIL_APP_PASSWORD>" `
  -FromAddress "farhadahmadi14132@gmail.com" `
  -FromName "CleanArchitecture Notifications"
```

نکته:
1. برای Gmail باید App Password استفاده کنی، نه پسورد اصلی اکانت.
2. اگر `Notifications:Email:Enabled=true` باشد ولی مقادیر SMTP ناقص باشند، برنامه در startup خطای واضح می‌دهد.
3. کلید `Notifications:SensitiveDataEncryptionKey` هم توسط اسکریپت ست می‌شود (اگر ندهی خودکار تولید می‌شود).

## 5) سرویس‌های جانبی
1. RabbitMQ: `RabbitMq:*`
2. Redis: `RedisCache:*`
3. Logging/Alerting: `Serilog:*`, `OperationalAlerting:*`
4. Scheduler: `Scheduler:*`
5. Outbox: `Outbox:*`

## 5.1) کانفیگ مهم Scheduler
1. `Scheduler:SeedDefaults`
2. `Scheduler:PollingIntervalSeconds`
3. `Scheduler:MaxDueJobsPerIteration`
4. `Scheduler:LockLeaseSeconds`
5. `Scheduler:MisfireGraceSeconds`
6. `Scheduler:DefaultQuarantineMinutes`
7. `Scheduler:NodeId`

## 5.2) کانفیگ مهم Outbox
1. `Outbox:BatchSize`
2. `Outbox:PollingIntervalSeconds`
3. `Outbox:MaxRetryCount`
4. `Outbox:RetryDelaySeconds`
5. `Outbox:MaxPendingMessages`

## 6) چک‌لیست قبل از Run
1. DB connection درست است.
2. JWT کامل است.
3. Notification encryption key تنظیم شده است.
4. اگر ارسال واقعی می‌خواهی، provider credentialها کامل است.
5. Scheduler و Outbox در محیط هدف تنظیم شده‌اند.
