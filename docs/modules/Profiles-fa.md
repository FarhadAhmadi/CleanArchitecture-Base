# ماژول Profiles

تاریخ به‌روزرسانی: 2026-02-21

## هدف
مدیریت پروفایل شخصی، عمومی و گزارش مدیریتی پروفایل کاربران.

## مسئولیت‌های اصلی
- ایجاد و خواندن پروفایل کاربر جاری
- به‌روزرسانی بخش‌های پروفایل: basic, bio, contact, privacy, preferences
- مدیریت social links، avatar، interests و music
- ارائه public profile
- گزارش مدیریتی پروفایل‌ها

## مدل دامنه
- `UserProfile`
- `UserProfileChangedDomainEvent`

## Use caseهای کلیدی
- `CreateMyProfileCommand`
- `GetMyProfileQuery`
- `UpdateMyProfileBasicCommand`
- `UpdateMyProfileBioCommand`
- `UpdateMyProfileContactCommand`
- `UpdateMyProfilePrivacyCommand`
- `UpdateMyProfilePreferencesCommand`
- `UpdateMyProfileSocialLinksCommand`
- `GetPublicProfileQuery`
- `GetProfilesAdminReportQuery`

## API و سطح دسترسی
- مسیرها: `/api/v1/profiles/*`
- خواندن: `profiles.read`
- نوشتن: `profiles.write`
- پروفایل عمومی: `profiles.public.read`
- گزارش ادمین: `profiles.admin.read`

## وابستگی‌ها
- ماژول Files برای avatar/music file id
- ماژول Logging و Notifications برای رویدادهای تغییر

## داده و نگهداشت
- اسکیما: `profiles`
- مدیریت فیلدهای privacy باید versioned و قابل ردگیری باشد.

## نکات عملیاتی
- تغییرات privacy و profile sharing نیازمند audit دقیق است.

## ریسک‌ها
- افزایش پیچیدگی policyهای privacy با رشد محصول.
