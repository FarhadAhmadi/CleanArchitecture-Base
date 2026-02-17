# راهنمای کامل راه‌اندازی، بهره‌برداری و Go-Live
**پروژه:** Clean Architecture Base  
**نسخه سند:** 2.0  
**تاریخ:** 2026-02-17  
**مخاطب:** تیم توسعه، DevOps، SRE، امنیت

---

## 1) هدف و دامنه
این راهنما اجرای سیستم را از صفر تا بهره‌برداری پوشش می‌دهد:
1. Local Development
2. Staging/Production-like
3. CI/CD و Infrastructure provisioning
4. امنیت، مانیتورینگ، DR و تست عملکرد

---

## 2) پیش‌نیازها
1. `dotnet 10.x`
2. `docker` + `docker compose`
3. `git`
4. `dotnet-ef`
5. اختیاری:
1. `kubectl`
2. `terraform`
3. `k6`
4. `sqlcmd`

نصب `dotnet-ef`:
```bash
dotnet tool install --global dotnet-ef
```

---

## 3) ساختار تنظیمات (Configuration Map)
کلیدهای اصلی:
1. `ConnectionStrings:Database`
2. `Jwt:*`
3. `AuthSecurity:*`
4. `ExternalOAuth:*`
5. `ApiSecurity:*`
6. `RabbitMq:*`
7. `RedisCache:*`
8. `PermissionCache:*`
9. `LoggingPlatform:*`
10. `OperationalSlo:*`
11. `OperationalAlerting:*`
12. `SecretManagement:*`
13. `DatabaseMigrations:*`

نکته:
1. در Production از `Key Vault`/secret manager استفاده شود.
2. `appsettings.*` فقط baseline هستند.

---

## 4) اجرای کامل Local (Developer Mode)
## 4.1 اجرای سرویس‌های وابسته
از ریشه پروژه:
```bash
docker compose up -d sqlserver seq elasticsearch kibana logstash rabbitmq redis
```

## 4.2 restore/build
```bash
dotnet restore CleanArchitecture.slnx
dotnet build CleanArchitecture.slnx
```

## 4.3 اعمال migration
```bash
dotnet ef database update \
  --project src/Infrastructure/Infrastructure.csproj \
  --startup-project src/Web.Api/Web.Api.csproj \
  --context Infrastructure.Database.ApplicationDbContext
```

## 4.4 اجرای API
```bash
dotnet run --project src/Web.Api/Web.Api.csproj
```

## 4.5 تست سریع سلامت
1. API health:
```bash
curl http://localhost:5000/health
```
2. Seq: `http://localhost:8081`
3. Kibana: `http://localhost:5601`
4. RabbitMQ: `http://localhost:15672`

---

## 5) راه‌اندازی OAuth2 (Google / Meta)
## 5.1 Google
1. ساخت OAuth app در Google Cloud
2. تعریف callback:
`https://<domain>/api/v1/users/oauth/callback`
3. تنظیم:
1. `ExternalOAuth:Google:Enabled=true`
2. `ClientId`
3. `ClientSecret`

## 5.2 Meta (Facebook)
1. ساخت app در Meta Developers
2. تعریف callback مشابه
3. تنظیم:
1. `ExternalOAuth:Meta:Enabled=true`
2. `AppId`
3. `AppSecret`

## 5.3 تست مسیر OAuth
1. شروع Google:
`GET /api/v1/users/oauth/google/start`
2. شروع Meta:
`GET /api/v1/users/oauth/meta/start`

---

## 6) راه‌اندازی امنیت (Security Hardening Checklist)
1. `UseHsts=true` در Production
2. `EnableSecurityHeaders=true`
3. `PerUserRateLimitPermitLimit` و `PerIpRateLimitPermitLimit` متناسب با ترافیک
4. `AuthSecurity:MaxFailedLoginAttempts` و `LockoutMinutes`
5. `Jwt:Secret` قوی و rotation با `PreviousSecrets`
6. OAuth secrets فقط در Vault
7. فعال‌سازی workflow امنیتی CI

---

## 7) راه‌اندازی Reliability (Outbox/Inbox/Broker)
1. `RabbitMq:Enabled=true`
2. Queue/Exchange مطابق تنظیمات appsettings
3. بررسی outbox health:
`/health` و `observability/metrics`
4. مانیتور backlog:
1. `OutboxPending`
2. `OutboxFailed`
3. `IngestionQueueDepth`

---

## 8) راه‌اندازی Cache
1. برای Production:
`RedisCache:Enabled=true`
2. اتصال:
`RedisCache:ConnectionString`
3. کنترل TTL:
`PermissionCache:AbsoluteExpirationSeconds`

fallback:
اگر Redis خاموش شود، سیستم به DistributedMemoryCache برمی‌گردد.

---

## 9) مانیتورینگ و Alerting
## 9.1 Observability endpoint
```text
GET /api/v1/observability/metrics
```

## 9.2 Alert worker
تنظیم:
1. `OperationalAlerting:Enabled=true`
2. `WebhookUrls`
3. `PagerDutyRoutingKey` (اختیاری)

## 9.3 لاگ امنیتی
رویدادهای زیر باید در SIEM ingest شوند:
1. Authentication failed/succeeded
2. Account locked
3. Authorization denied
4. JWT challenge/fail

---

## 10) CI/CD - دستورالعمل عملیاتی
## 10.1 CI
فایل: `.github/workflows/build.yml`
مراحل:
1. restore
2. build
3. format check
4. test
5. publish
6. migration script artifact
7. image scan + push

## 10.2 Security CI
فایل: `.github/workflows/security.yml`
1. CodeQL
2. dependency vulnerability scan
3. secret scan

## 10.3 CD
فایل: `.github/workflows/cd.yml`
1. AKS context
2. migration update
3. blue/green deploy
4. smoke test
5. rollback selector on failure

---

## 11) IaC - Terraform
مسیر:
`infra/terraform`

## 11.1 اجرای plan
```bash
cd infra/terraform
terraform init
terraform validate
terraform plan -var-file=environments/dev.tfvars
```

## 11.2 اجرای apply
```bash
terraform apply -var-file=environments/dev.tfvars
```

محیط‌ها:
1. `dev.tfvars`
2. `staging.tfvars`
3. `prod.tfvars`

---

## 12) Blue/Green Release Runbook
اسکریپت:
`deploy/k8s/blue-green-deploy.sh`

ورودی‌ها:
1. namespace
2. image tag
3. image repository
4. smoke path

مراحل:
1. تشخیص track فعال
2. deploy track غیرفعال
3. rollout و readiness
4. smoke test روی track جدید
5. switch service selector
6. scale-down track قدیمی

---

## 13) Disaster Recovery Runbook
اسکریپت‌ها:
1. `scripts/dr/backup.ps1`
2. `scripts/dr/restore.ps1`
3. `scripts/dr/drill.ps1`

Workflow:
`.github/workflows/dr-drill.yml`

الزام:
1. اجرای drill دوره‌ای
2. ثبت RPO/RTO
3. ایجاد action item در صورت breach

---

## 14) Performance Validation
اسکریپت‌های k6:
1. `tests/Performance/k6/load-test.js`
2. `tests/Performance/k6/soak-test.js`

اجرای دستی:
```bash
k6 run -e BASE_URL=http://localhost:5000 tests/Performance/k6/load-test.js
```

---

## 15) چک‌لیست Go-Live (Final Gate)
1. migrationها روی محیط مقصد اعمال شده‌اند.
2. secrets کامل و از Vault خوانده می‌شوند.
3. OAuth callbackها verify شده‌اند.
4. health endpoint و metrics endpoint سبز هستند.
5. alert delivery test موفق است.
6. DR drill در 30 روز اخیر موفق بوده است.
7. smoke test پس از آخرین deploy پاس شده است.
8. rollback تمرینی موفق بوده است.

---

## 16) Troubleshooting سریع
1. `401/403` ناگهانی:
1. JWT issuer/audience/secret را چک کن.
2. permission cache version را بررسی کن.

2. OAuth callback error:
1. redirect URI mismatch
2. provider secret/callback misconfig

3. Outbox backlog:
1. RabbitMQ availability
2. worker logs
3. retry thresholds

4. High auth failures:
1. lockout metrics
2. rate limit hits
3. suspicious IP patterns

---

## 17) مسئولیت تیمی (Ownership پیشنهادی)
1. تیم Backend: Application/Domain/Endpoint correctness
2. تیم DevOps: CI/CD, IaC, K8s, DR automation
3. تیم Security: policy, secrets, scans, incident workflow
4. تیم SRE: SLO/alert tuning, capacity and reliability

---

## 18) ضمیمه: دستورات پرکاربرد
### Build/Test
```bash
dotnet build CleanArchitecture.slnx
dotnet test CleanArchitecture.slnx --no-build
```

### Migration add/update
```bash
dotnet ef migrations add <Name> \
  --project src/Infrastructure/Infrastructure.csproj \
  --startup-project src/Web.Api/Web.Api.csproj \
  --context Infrastructure.Database.ApplicationDbContext

dotnet ef database update \
  --project src/Infrastructure/Infrastructure.csproj \
  --startup-project src/Web.Api/Web.Api.csproj \
  --context Infrastructure.Database.ApplicationDbContext
```

### Run API
```bash
dotnet run --project src/Web.Api/Web.Api.csproj
```
