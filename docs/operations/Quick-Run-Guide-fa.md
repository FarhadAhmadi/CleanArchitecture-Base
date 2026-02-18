# راهنمای اجرای سریع پروژه

هدف این فایل: بدون درگیر شدن با جزئیات، کل پروژه را سریع بالا بیاوری.

لیست کامل کانفیگ‌های لازم:
`docs/operations/Run-Required-Config-fa.md`

## پیش‌نیاز
- Docker Desktop
- .NET SDK (نسخه پروژه)
- PowerShell

## اجرای کامل با یک دستور
از ریشه پروژه:

```powershell
.\scripts\dev\dev-stack.ps1 up
```

یا روی ویندوز:

```cmd
run-dev.cmd
```

این دستور کارهای زیر را انجام می‌دهد:
- `dotnet build`
- `docker compose up -d --build` برای تمام سرویس‌ها
- چک وضعیت کانتینرها
- اجرای تست سریع

## دستورات روزمره

```powershell
.\scripts\dev\dev-stack.ps1 status
.\scripts\dev\dev-stack.ps1 logs
.\scripts\dev\dev-stack.ps1 down
.\scripts\dev\dev-stack.ps1 restart
```

## حالت سریع‌تر (بدون build یا test)

```powershell
.\scripts\dev\dev-stack.ps1 up -SkipBuild -SkipTests
```

## آدرس‌ها بعد از بالا آمدن
- API/Swagger: `http://localhost:5000/swagger`
- Health: `http://localhost:5000/health`
- Seq: `http://localhost:8081`
- Kibana: `http://localhost:5601`
- RabbitMQ: `http://localhost:15672`
- MinIO API: `http://localhost:9000`
- MinIO Console: `http://localhost:9001`
