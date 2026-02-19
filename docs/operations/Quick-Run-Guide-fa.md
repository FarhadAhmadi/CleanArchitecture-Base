# راهنمای اجرای سریع پروژه

این راهنما برای اجرای پروژه با کمترین درگیری طراحی شده است.

## سریع‌ترین مسیر
از ریشه پروژه اجرا کن:

```powershell
.\scripts\dev\dev-stack.ps1 up
```

یا در ویندوز:

```cmd
run-dev.cmd
```

## دستورات کاربردی
```powershell
.\scripts\dev\dev-stack.ps1 status
.\scripts\dev\dev-stack.ps1 logs
.\scripts\dev\dev-stack.ps1 down
.\scripts\dev\dev-stack.ps1 restart
```

## حالت سریع‌تر
```powershell
.\scripts\dev\dev-stack.ps1 up -SkipBuild -SkipTests
```

## نکته Redis
در Compose فعلی، Redis فقط داخل شبکه Docker در دسترس است (`redis:6379`) و روی host publish نمی‌شود تا خطاهای bind پورت در ویندوز کمتر شود.

## خطای پورت (ports are not available)
اگر این خطا را دیدی، یعنی یک پورت host آزاد نیست یا توسط Windows رزرو شده است.  
اسکریپت `dev-stack.ps1` قبل از `docker compose up` پورت‌های موردنیاز را pre-check می‌کند و دلیل دقیق‌تری نشان می‌دهد.

## لینک‌های تکمیلی
1. کانفیگ‌های لازم: `docs/operations/Run-Required-Config-fa.md`
2. راهنمای کامل setup: `docs/operations/System-Setup-Guide-fa.md`
3. تنظیم سریع SMTP ایمیل: `scripts/dev/configure-email.ps1`
