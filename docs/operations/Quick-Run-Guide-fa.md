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

## لینک‌های تکمیلی
1. کانفیگ‌های لازم: `docs/operations/Run-Required-Config-fa.md`
2. راهنمای کامل setup: `docs/operations/System-Setup-Guide-fa.md`
