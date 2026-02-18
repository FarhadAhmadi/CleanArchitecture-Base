# ساختار Modular Monolith (نسخه به‌روز)

**پروژه:** Clean Architecture Base  
**تاریخ به‌روزرسانی:** 2026-02-18

## 1) هدف
این سند ساختار فعلی پروژه را ثبت می‌کند تا توسعه همه ماژول‌ها با یک الگوی واحد انجام شود.

## 2) ساختار فعلی لایه‌ها

### Domain
- `src/Domain/Modules/Auditing`
- `src/Domain/Modules/Authorization`
- `src/Domain/Modules/Files`
- `src/Domain/Modules/Logging`
- `src/Domain/Modules/Notifications`
- `src/Domain/Modules/Todos`
- `src/Domain/Modules/Users`

### Application
- `src/Application/Modules/Authorization`
- `src/Application/Modules/Todos`
- `src/Application/Modules/Users`
- لایه مشترک: `src/Application/Abstractions`

### Infrastructure
- `src/Infrastructure/Modules/Audit/Infrastructure/Auditing`
- `src/Infrastructure/Modules/Auth/Infrastructure/Authentication`
- `src/Infrastructure/Modules/Authorization/Infrastructure/Authorization`
- `src/Infrastructure/Modules/Files/Infrastructure/Files`
- `src/Infrastructure/Modules/Logging/Infrastructure/Logging`
- `src/Infrastructure/Modules/Notifications/Infrastructure/Notifications`
- `src/Infrastructure/Modules/Todos/Infrastructure/Todos`
- `src/Infrastructure/Modules/Users/Infrastructure/Users`
- تنظیمات EF همه ماژول‌ها: `src/Infrastructure/Database/Configurations`

### Web API
- `src/Web.Api/Endpoints/Modules/Audit`
- `src/Web.Api/Endpoints/Modules/Authorization`
- `src/Web.Api/Endpoints/Modules/Files`
- `src/Web.Api/Endpoints/Modules/Logging`
- `src/Web.Api/Endpoints/Modules/Notifications`
- `src/Web.Api/Endpoints/Modules/Observability`
- `src/Web.Api/Endpoints/Modules/Todos`
- `src/Web.Api/Endpoints/Modules/Users`

## 3) قوانین توسعه
1. هر قابلیت جدید در مسیر ماژول خودش ایجاد شود.
2. هیچ کد Domain از Infrastructure استفاده نکند.
3. کدهای cross-cutting فقط در مسیرهای مشترک بمانند.
4. هر Endpoint جدید در `Endpoints/Modules/<Module>` قرار بگیرد.
5. هر Feature جدید Application در `Application/Modules/<Module>/<Feature>` ساخته شود.

## 4) Composition Root
- `src/Infrastructure/DependencyInjection.cs` orchestration سطح بالا را نگه می‌دارد.
- ثبت سرویس‌ها به‌صورت module-based در `src/Infrastructure/Modules/*Module.cs` انجام می‌شود.

## 5) Scaffolding
اسکریپت فیچر جدید با ساختار ماژولی همسو است:
- `scripts/feature/new-feature.ps1`

## 6) وضعیت جاری
- ساختار پوشه‌ای ماژولی اعمال شده است.
- همه `EntityTypeConfiguration`‌ها متمرکز شده‌اند.
- build/test روی ساختار جدید پاس است.
