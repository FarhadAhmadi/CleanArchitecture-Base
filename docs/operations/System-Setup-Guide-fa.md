# راهنمای کامل راه‌اندازی سیستم (نسخه به‌روز)

**پروژه:** Clean Architecture Base  
**تاریخ:** 2026-02-18

## 1) پیش‌نیازها
1. Docker Desktop
2. .NET SDK (مطابق `Directory.Build.props`)
3. PowerShell
4. اختیاری: `dotnet-ef`, `k6`, `terraform`

## 2) اجرای سریع پیشنهادی
از ریشه پروژه:

```powershell
.\scripts\dev\dev-stack.ps1 up
```

این دستور:
1. build می‌گیرد
2. سرویس‌ها را بالا می‌آورد
3. وضعیت کانتینرها را چک می‌کند
4. تست سریع اجرا می‌کند

## 3) اجرای دستی

### 3.1 بالا آوردن سرویس‌های وابسته
```powershell
docker compose up -d sqlserver rabbitmq redis minio seq elasticsearch kibana logstash
```
نکته: Redis در این setup روی host publish نمی‌شود و فقط با آدرس `redis:6379` داخل شبکه Docker مصرف می‌شود.

### 3.2 build
```powershell
dotnet build CleanArchitecture.slnx -c Debug
```

### 3.3 migration
```powershell
dotnet ef database update --project src/Infrastructure/Infrastructure.csproj --startup-project src/Web.Api/Web.Api.csproj --context Infrastructure.Database.ApplicationDbContext
```

### 3.4 run
```powershell
dotnet run --project src/Web.Api/Web.Api.csproj
```

## 4) آدرس‌های مهم
1. Swagger: `http://localhost:5000/swagger`
2. Health: `http://localhost:5000/health`
3. Seq: `http://localhost:8081`
4. Kibana: `http://localhost:5601`
5. RabbitMQ: `http://localhost:15672`
6. MinIO Console: `http://localhost:9001`
7. Scheduler APIs: `http://localhost:5000/api/v1/scheduler/*`

## 5) کانفیگ‌های ضروری
جزئیات کامل در:
- `docs/operations/Run-Required-Config-fa.md`

حداقل لازم:
1. `ConnectionStrings:Database`
2. `Jwt:Secret`, `Jwt:Issuer`, `Jwt:Audience`
3. `Notifications:SensitiveDataEncryptionKey`

## 6) ساختار ماژولی فعلی
- Domain: `src/Domain/Modules/*`
- Application: `src/Application/Modules/*`
- Infrastructure: `src/Infrastructure/Modules/*`
- API Endpoints: `src/Web.Api/Endpoints/Modules/*`

## 7) کنترل کیفیت
1. `dotnet format --verify-no-changes`
2. `dotnet build CleanArchitecture.slnx -c Debug`
3. `dotnet test CleanArchitecture.slnx -c Debug --no-build`

## 8) عیب‌یابی سریع
1. خطای DB: connection string و `sqlserver` container را چک کن.
2. خطای JWT: `Jwt:*` را بررسی کن.
3. اعلان ارسال نمی‌شود: تنظیمات provider در `Notifications:*` را کامل کن.
4. مشکل MinIO: `FileStorage:*` و دسترسی bucket را چک کن.
5. خطای `ports are not available`: معمولاً یکی از پورت‌های host در حال استفاده یا داخل excluded range ویندوز است؛ خروجی pre-check اسکریپت `scripts/dev/dev-stack.ps1` را بررسی کن.

## 9) گام‌های اعتبارسنجی بعد از Setup
1. یک کاربر ثبت‌نام و login کن.
2. یک job زمان‌بند ایجاد کن (`POST /api/v1/scheduler/jobs`).
3. برای job زمان‌بندی ثبت کن (`POST /api/v1/scheduler/jobs/{jobId}/schedule`).
4. وضعیت اجرا را در `GET /api/v1/scheduler/jobs/logs` بررسی کن.
5. یک اعلان آزمایشی بساز و در `GET /api/v1/notifications/reports/summary` نتیجه را بررسی کن.
