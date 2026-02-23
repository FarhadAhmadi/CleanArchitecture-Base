# Security Baseline (Best Practice)

این سند وضعیت امنیتی پروژه را به شکل اجرایی نگه می‌دارد و نشان می‌دهد هر کنترل در کد، CI/CD یا عملیات چگونه پوشش داده شده است.

## 1) احراز هویت، نشست و دسترسی

- احراز هویت استاندارد:
  - JWT + OAuth providerهای خارجی (`Google`/`Meta`) پیاده سازی شده.
- سیاست گذرواژه قوی:
  - حداقل طول، پیچیدگی، حداقل کاراکتر یکتا و deny-list پسوردهای رایج/نشتی اعمال می‌شود.
  - مسیرها: `src/Infrastructure/Modules/Auth/Infrastructure/Authentication/PasswordPolicyValidator.cs`
- جلوگیری از reuse پسورد:
  - تاریخچه رمز عبور در دیتابیس نگهداری و enforce می‌شود.
  - مسیرها: `src/Domain/Modules/Users/UserPasswordHistory.cs`
- مدیریت نشست:
  - refresh token rotation + reuse detection + revoke individual/all sessions.
  - reset password باعث revoke توکن‌های فعال می‌شود.
- اصل کمترین دسترسی:
  - RBAC/permission-based authorization در کل endpointها.

## 2) امنیت API و ورودی/خروجی

- Validation سمت سرور:
  - FluentValidation + policyهای قوی برای سناریوهای auth.
- Hardening درخواست:
  - محدودیت سایز/تعداد هدرها، content-type enforcement، reject متدهای TRACE/TRACK.
- امنیت پاسخ HTTP:
  - CSP, X-Frame-Options, X-Content-Type-Options, Referrer-Policy, COOP/COEP/CORP.
- CORS:
  - fail-closed در production در صورت نبود origin مجاز.
- Rate limiting:
  - global + per-user/per-ip + policy اختصاصی لینک عمومی فایل.

## 3) رمزنگاری، اسرار و کلید

- رمزنگاری در انتقال: TLS/HSTS در production.
- مدیریت اسرار:
  - Azure Key Vault پشتیبانی می‌شود.
  - اعتبارسنجی startup برای جلوگیری از placeholder secret در محیط غیر-development فعال است.
- تولید تصادفی امن:
  - `RandomNumberGenerator` برای token/code استفاده شده است.

## 4) لاگینگ، مانیتورینگ و پاسخ حادثه

- لاگ امنیتی:
  - رویدادهای auth/authz و account lock ثبت می‌شود.
- عدم نشت داده حساس در لاگ:
  - query string قبل از log redaction می‌شود (token/password/code/...).
- runbookها:
  - incident/dr/backup runbookها موجود هستند.

## 5) امنیت زنجیره تامین و CI/CD

- SAST: CodeQL
- SCA/Dependency scan: NuGet vulnerability + Trivy
- Secret scanning: Gitleaks
- DAST (API baseline): OWASP ZAP API scan روی OpenAPI snapshot
- Dependabot: بروزرسانی روزانه NuGet

## 6) VDP / گزارش آسیب‌پذیری

- سیاست گزارش آسیب‌پذیری در `SECURITY.md` تعریف شده است.

## 7) موارد خارج از scope مستقیم کد

موارد زیر نیازمند کنترل سطح زیرساخت/سازمان است و باید در محیط عملیاتی enforce شود:

- WAF, network segmentation, host hardening
- mTLS service-to-service
- periodic pentest
- backup encryption verification و drillهای دوره‌ای
- secure mobile token storage / anti-reverse-engineering
- secure coding training برنامه‌دار
- data classification, retention, legal compliance (مانند GDPR در صورت نیاز)
