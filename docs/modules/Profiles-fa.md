# ماژول Profiles

تاریخ به‌روزرسانی: 2026-02-21

## هدف
مدیریت پروفایل شخصی و عمومی کاربر با قابلیت گزارش مدیریتی.

## ترتیب IOrderedEndpoint
این ماژول از `IOrderedEndpoint` استفاده نمی‌کند.

## کاتالوگ کامل Endpointها
| Method | Path | دسترسی | دلیل وجود | ورودی‌ها |
|---|---|---|---|---|
| POST | `/api/v1/profiles/me` | `profiles.write` | ایجاد پروفایل کاربر جاری | Body: `displayName`, `preferredLanguage`, `isProfilePublic` |
| GET | `/api/v1/profiles/me` | `profiles.read` | دریافت پروفایل کاربر جاری | - |
| PUT | `/api/v1/profiles/me/basic` | `profiles.write` | بروزرسانی اطلاعات پایه | Body: `displayName`, `bio`, `dateOfBirth`, `gender`, `location` |
| PATCH | `/api/v1/profiles/me/bio` | `profiles.write` | بروزرسانی بیو | Body: `bio` |
| PATCH | `/api/v1/profiles/me/contact` | `profiles.write` | بروزرسانی اطلاعات تماس | Body: `contactEmail`, `contactPhone`, `website`, `timeZone` |
| PATCH | `/api/v1/profiles/me/privacy` | `profiles.write` | بروزرسانی سیاست حریم خصوصی | Body: `isProfilePublic`, `showEmail`, `showPhone` |
| PATCH | `/api/v1/profiles/me/preferences` | `profiles.write` | بروزرسانی ترجیحات | Body: `preferredLanguage`, `receiveSecurityAlerts`, `receiveProductUpdates` |
| PATCH | `/api/v1/profiles/me/social-links` | `profiles.write` | بروزرسانی لینک‌های اجتماعی | Body: `links: Dictionary<string,string>` |
| GET | `/api/v1/profiles/me/music` | `profiles.read` | دریافت اطلاعات موزیک | - |
| PUT | `/api/v1/profiles/me/music` | `profiles.write` | بروزرسانی موزیک پروفایل | Body: `musicTitle`, `musicArtist`, `musicFileId` |
| POST | `/api/v1/profiles/me/interests` | `profiles.write` | افزودن علاقه‌مندی‌ها | Body: `interests[]` |
| DELETE | `/api/v1/profiles/me/interests/{interest}` | `profiles.write` | حذف یک علاقه‌مندی | Path: `interest` |
| PUT | `/api/v1/profiles/me/avatar` | `profiles.write` | تنظیم avatar | Body: `avatarFileId` |
| DELETE | `/api/v1/profiles/me/avatar` | `profiles.write` | حذف avatar | - |
| GET | `/api/v1/profiles/{userId:guid}/public` | `profiles.public.read` | نمایش پروفایل عمومی کاربر | Path: `userId` |
| GET | `/api/v1/profiles/reports/admin` | `profiles.admin.read` | گزارش مدیریتی پروفایل‌ها | Query: paging + `search`, `isProfilePublic`, `preferredLanguage`, `minCompleteness`, `maxCompleteness`, `updatedFrom`, `updatedTo` |

## نکات طراحی مهم
- privacy controls به‌صورت endpoint جدا تعریف شده تا governance ساده‌تر باشد.
- avatar/music به ماژول Files وابسته است (شناسه فایل).
- endpoint گزارش ادمین برای مانیتور کیفیت داده پروفایل طراحی شده است.

## وابستگی‌ها
- Files
- Logging/Notifications (رویدادهای تغییر)
- schema `profiles`

## سناریوهای خطا
- file id نامعتبر در avatar/music
- نقض policy حریم خصوصی
- پارامترهای بازه گزارش نامعتبر
