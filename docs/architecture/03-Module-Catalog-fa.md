# کاتالوگ ماژول‌ها (نسخه تفکیکی)

تاریخ: 2026-02-21  
مبنای تحلیل: commit `6db4c27`

## هدف
این سند نمای فشرده ماژول‌ها را می‌دهد و مستندات کامل هر ماژول را به صورت تفکیکی ارجاع می‌دهد.

## ساختار تفکیکی جدید
- ایندکس ماژول‌ها: `docs/modules/README-fa.md`
- روند استفاده و Workflow همه ماژول‌ها: `docs/modules/Usage-and-Workflows-fa.md`
- ماژول‌های کسب‌وکاری:
  - `docs/modules/Users-Auth-fa.md`
  - `docs/modules/Authorization-fa.md`
  - `docs/modules/Todos-fa.md`
  - `docs/modules/Profiles-fa.md`
  - `docs/modules/Files-fa.md`
  - `docs/modules/Notifications-fa.md`
  - `docs/modules/Logging-fa.md`
  - `docs/modules/Audit-fa.md`
  - `docs/modules/Observability-fa.md`
  - `docs/modules/Scheduler-fa.md`
- ماژول مشترک:
  - `docs/modules/Shared-Platform-fa.md`

## خلاصه وضعیت ماژول‌ها
| ماژول | وضعیت | حوزه |
|---|---|---|
| Users/Auth | فعال | هویت، احراز هویت، نشست |
| Authorization | فعال | RBAC و Permission |
| Todos | فعال | مدیریت تسک شخصی |
| Profiles | فعال | مدیریت پروفایل |
| Files | فعال | مدیریت فایل و ACL |
| Notifications | فعال | اعلان چندکاناله |
| Logging | فعال | ingest/alert/access |
| Audit | فعال | trail tamper-evident |
| Observability | فعال | KPI و replay عملیاتی |
| Scheduler | فعال | زمان‌بندی و اجرای Job |
| Shared Platform | فعال | زیرساخت مشترک |

## نکته
نسخه قبلی این سند شامل توضیح کامل همه ماژول‌ها در یک فایل بود. از این تاریخ، توضیح کامل به فایل‌های مستقل هر ماژول منتقل شده تا نگهداری ساده‌تر و دقیق‌تر شود.
