# ماژول Users/Auth

تاریخ به‌روزرسانی: 2026-02-21

## هدف
مدیریت چرخه کامل هویت کاربر شامل ثبت‌نام، ورود، مدیریت نشست‌ها، بازیابی رمز، تایید ایمیل و OAuth.

## مسئولیت‌های اصلی
- ثبت‌نام کاربر و تخصیص نقش اولیه
- صدور توکن دسترسی و Refresh Token
- چرخش و ابطال Refresh Token (تکی و همه نشست‌ها)
- تایید ایمیل و ارسال مجدد کد تایید
- بازیابی/ریست رمز عبور
- ورود OAuth با providerهای بیرونی

## مدل دامنه
- `User`
- `RefreshToken`
- `UserExternalLogin`
- `UserRegisteredDomainEvent`

## Use caseهای کلیدی
- `RegisterUserCommand`
- `LoginUserCommand`
- `RefreshTokenCommand`
- `RevokeRefreshTokenCommand`
- `RevokeAllSessionsCommand`
- `VerifyEmailCommand`
- `RequestPasswordResetCommand`
- `ConfirmPasswordResetCommand`
- `CompleteOAuthCommand`

## API و سطح دسترسی
- مسیرها: `/api/v1/users/*`
- عمومی: `register`, `login`, `refresh`, `revoke`, `forgot-password`, `reset-password`, `verify-email`, `oauth/*`
- نیازمند احراز هویت: `GET /users/me`, `POST /users/sessions/revoke-all`
- نیازمند مجوز: `GET /users/{userId}` با `users:access`

## وابستگی‌ها
- Identity Core و JWT
- ماژول Notification برای کد تایید و ریست
- ماژول Audit برای ثبت عملیات حساس امنیتی

## داده و نگهداشت
- اسکیما: `users` و `auth`
- نکات مهم: lockout، expiry policy، token reuse detection

## نکات عملیاتی
- کانفیگ صحیح کلید JWT و زمان انقضا الزامی است.
- فعال بودن providerهای OAuth باید با secret امن انجام شود.

## ریسک‌ها
- تنظیم نادرست JWT/Refresh می‌تواند باعث افزایش ریسک session hijack شود.
- شکست ارسال اعلان، مسیر تایید ایمیل و ریست رمز را مختل می‌کند.
