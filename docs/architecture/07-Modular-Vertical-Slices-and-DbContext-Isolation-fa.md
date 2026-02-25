# 07) معماری ماژولار + Vertical Slices + جداسازی DbContext (مرحله فعلی)

تاریخ: 2026-02-25

## تصمیم معماری
- سیستم به صورت `Modular Monolith` نگه‌داری می‌شود.
- داخل هر ماژول، توسعه به صورت `Vertical Slice` انجام می‌شود.
- پایگاه داده فعلا `Shared DB` است، اما در سطح کد، دسترسی دیتابیس به صورت `Module-scoped DbContext` جدا شده است.

## ساختار عملیاتی فعلی
- کد ماژولی: `src/Modules/<Module>/{Api,Application,Domain,Infrastructure}`
- کد مشترک: `src/Shared/{Api,Application,Infrastructure,Kernel}`
- تست‌ها:
- `tests/ArchitectureTests/Features/*` برای قواعد ماژولی
- `tests/ArchitectureTests/Shared/*` برای قواعد مشترک

## جداسازی دیتابیس در سطح Context
- Contextهای اصلی همچنان:
- `ApplicationDbContext` (write)
- `ApplicationReadDbContext` (read)
- برای هر ماژول یک Adapter Context اضافه شده تا مرز ماژول شفاف بماند:
- `UsersWriteDbContext / UsersReadDbContext`
- `AuthorizationWriteDbContext / AuthorizationReadDbContext`
- `TodosWriteDbContext / TodosReadDbContext`
- `AuditWriteDbContext / AuditReadDbContext`
- `LoggingWriteDbContext / LoggingReadDbContext`
- `ProfilesWriteDbContext / ProfilesReadDbContext`
- `NotificationsWriteDbContext / NotificationsReadDbContext`
- `FilesWriteDbContext / FilesReadDbContext`
- `SchedulerWriteDbContext / SchedulerReadDbContext`

این Adapterها در `DataAccessModule` رجیستر شده‌اند و اینترفیس‌های `I*WriteDbContext` و `I*ReadDbContext` را پوشش می‌دهند.

## قوانین فعلی وابستگی
- Application و Api هر ماژول نباید مستقیم از `ApplicationDbContext` یا `ApplicationReadDbContext` استفاده کنند.
- استفاده باید فقط از پورت‌های ماژولی (`IUsersWriteDbContext`, ...) باشد.
- تست معماری `DbContextIsolationTests` این قانون را enforce می‌کند.

## وضعیت مهاجرت DB-Per-Module
این مرحله، پیش‌نیاز DB-per-module است اما هنوز DB فیزیکی جدا نشده است.

گام بعدی (اختیاری):
1. تعریف Connection String مستقل برای هر ماژول.
2. تبدیل Adapterها به DbContext واقعی ماژولی.
3. Migration مستقل برای هر ماژول.
4. اعمال integration event / outbox برای consistency بین ماژول‌ها.
