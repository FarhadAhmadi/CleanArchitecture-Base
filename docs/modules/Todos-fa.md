# ماژول Todos

تاریخ به‌روزرسانی: 2026-02-21

## هدف
مدیریت کارهای شخصی کاربر با عملیات CRUD و قابلیت جستجو/صفحه‌بندی.

## مسئولیت‌های اصلی
- ایجاد، دریافت لیست، دریافت موردی
- تکمیل، حذف، کپی تسک
- فیلتر، جستجو و مرتب‌سازی

## مدل دامنه
- `TodoItem`
- `Priority`
- رویدادهای دامنه‌ای: `Created`, `Completed`, `Deleted`

## Use caseهای کلیدی
- `CreateTodoCommand`
- `GetTodosQuery`
- `GetTodoByIdQuery`
- `CompleteTodoCommand`
- `DeleteTodoCommand`
- `CopyTodoCommand`

## API و سطح دسترسی
- مسیرها: `/api/v1/todos/*`
- خواندن: `todos:read`
- نوشتن: `todos:write`

## وابستگی‌ها
- user context برای owner-based access
- EF Core برای persistence

## داده و نگهداشت
- اسکیما: `todos`

## نکات عملیاتی
- برای داده حجیم، محدودیت page size باید enforce شود.

## ریسک‌ها
- پایین؛ ماژول سبک با coupling محدود به سایر بخش‌ها.
