# ماژول Authorization

تاریخ به‌روزرسانی: 2026-02-21

## هدف
پیاده‌سازی مدل کنترل دسترسی مبتنی بر Role و Permission برای کل سیستم.

## مسئولیت‌های اصلی
- تخصیص نقش به کاربر
- تخصیص مجوز به نقش
- ارائه ماتریس دسترسی (Access Control Matrix)
- ساخت Policy پویا بر اساس کد مجوز

## مدل دامنه
- `Role`
- `Permission`
- `UserRole`
- `RolePermission`
- `UserPermission`

## Use caseهای کلیدی
- `AssignRoleToUserCommand`
- `AssignPermissionToRoleCommand`
- `GetAccessControlQuery`

## API و سطح دسترسی
- مسیرها: `/api/v1/authorization/*`
- کل APIهای این ماژول نیازمند `authorization:manage`

## وابستگی‌ها
- لایه caching برای versioning مجوزها (Redis/Memory)
- ماژول Audit برای ردگیری تغییرات سیاست دسترسی

## داده و نگهداشت
- اسکیما: `auth`
- هر تغییر در نقش/مجوز باید cache invalidation درست داشته باشد.

## نکات عملیاتی
- بازسازی cache permission بعد از هر تغییر اجباری است.
- دسترسی endpointهای مدیریت نقش فقط برای ادمین‌های محدود تعریف شود.

## ریسک‌ها
- رشد ماتریس نقش/مجوز در سازمان بزرگ می‌تواند پیچیدگی عملیاتی ایجاد کند.
- ناهماهنگی کش مجوز با دیتابیس می‌تواند باعث خطای دسترسی شود.
