# ماژول Users/Auth

تاریخ به‌روزرسانی: 2026-02-21

## هدف
مدیریت هویت کاربر، احراز هویت، نشست‌ها و بازیابی دسترسی.

## ترتیب IOrderedEndpoint
این ماژول از `IOrderedEndpoint` استفاده نمی‌کند. ترتیب نهایی endpointها مطابق ترتیب کشف/ثبت endpointهای `IEndpoint` در startup است.

## کاتالوگ کامل Endpointها
| Method | Path | دسترسی | دلیل وجود | ورودی‌ها |
|---|---|---|---|---|
| POST | `/api/v1/users/register` | عمومی | ساخت حساب کاربر جدید | Body: `email`, `firstName`, `lastName`, `password` |
| POST | `/api/v1/users/login` | عمومی | صدور access/refresh token | Body: `email`, `password` |
| POST | `/api/v1/users/refresh` | عمومی | تمدید نشست با refresh token | Body: `refreshToken` |
| POST | `/api/v1/users/revoke` | عمومی | ابطال یک refresh token | Body: `refreshToken` |
| POST | `/api/v1/users/sessions/revoke-all` | کاربر احرازشده | ابطال همه نشست‌های فعال کاربر | Body: ندارد |
| POST | `/api/v1/users/forgot-password` | عمومی | شروع بازیابی رمز | Body: `email` |
| POST | `/api/v1/users/reset-password` | عمومی | اعمال رمز جدید با کد | Body: `email`, `code`, `newPassword` |
| POST | `/api/v1/users/verify-email` | عمومی | تایید ایمیل کاربر | Body: `email`, `code` |
| POST | `/api/v1/users/resend-verification-code` | عمومی | ارسال مجدد کد تایید | Body: `email` |
| GET | `/api/v1/users/oauth/{provider}/start` | عمومی | شروع OAuth flow | Path: `provider`، Query: `returnUrl` |
| GET | `/api/v1/users/oauth/callback` | عمومی | تکمیل OAuth و صدور token | Query/context برگشتی provider |
| GET | `/api/v1/users/me` | کاربر احرازشده | دریافت پروفایل پایه کاربر جاری | Body/Query: ندارد |
| GET | `/api/v1/users/{userId}` | `users:access` | دریافت اطلاعات کاربر با شناسه | Path: `userId` |

## نکات طراحی مهم
- `register` و `login` عملیات audit ثبت می‌کنند.
- `register` هدرهای `X-Verification-Required` و `X-Verification-Expires-At-Utc` را برمی‌گرداند.
- `refresh/revoke` برای کنترل lifecycle نشست‌ها استفاده می‌شود.
- endpointهای OAuth برای providerهای خارجی طراحی شده‌اند.

## مدل ورودی‌های مهم
- `Register.Request`: ایمیل، نام، نام‌خانوادگی، رمز
- `Login.Request`: ایمیل، رمز
- `Refresh.Request`: refresh token
- `Revoke.Request`: refresh token
- `ForgotPasswordRequest`: ایمیل
- `ResetPasswordRequest`: ایمیل، کد، رمز جدید
- `VerifyEmailRequest`: ایمیل، کد
- `ResendVerificationCodeRequest`: ایمیل

## وابستگی‌ها
- Notification برای ارسال کدها
- Audit برای ردگیری امنیتی
- JWT + Refresh token storage در `auth/users` schema

## سناریوهای خطا
- کد تایید منقضی/نامعتبر
- refresh token revoked/reused
- provider OAuth نامعتبر یا callback ناقص
