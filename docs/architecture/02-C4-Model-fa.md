# C4 Model (نمای استاندارد سریع)

تاریخ: 2026-02-21  
مبنای تحلیل: commit `6db4c27`

## 1) C4 - سطح Context

```mermaid
flowchart LR
    EndUser[کاربر نهایی]
    Admin[ادمین / اپراتور]
    API["CleanArchitecture Web API<br/>(Modular Monolith)"]

    OAuth[Google / Meta OAuth]
    KeyVault["Azure Key Vault<br/>(optional)"]
    Webhook["Webhook / PagerDuty<br/>(optional)"]

    EndUser --> API
    Admin --> API
    API --> OAuth
    API --> KeyVault
    API --> Webhook
```

### توضیح
- همه قابلیت های کسب وکاری در یک سیستم مرکزی ارائه می شوند.
- وابستگی های بیرونی به صورت optional/configurable هستند.

## 2) C4 - سطح Container

```mermaid
flowchart TB
    User[Users/Admins]

    subgraph System[CleanArchitecture Solution]
        WebApi["Web.Api<br/>ASP.NET Core Minimal API"]
        AppLayer["Application Layer<br/>CQRS + Validators + Behaviors"]
        Domain["Domain Layer<br/>Entities + Domain Events"]
        Infra["Infrastructure Layer<br/>Auth, Files, Notifications, Logging, Integration"]
        Workers["Background Workers<br/>Outbox/Inbox/Alerts/Notifications"]
    end

    SQL[(SQL Server)]
    Redis[(Redis Cache)]
    Rabbit[(RabbitMQ)]
    MinIO[(MinIO Object Storage)]
    Seq[(Seq)]
    Elastic[(Elasticsearch/Kibana)]

    User --> WebApi
    WebApi --> AppLayer
    AppLayer --> Domain
    AppLayer --> Infra
    Infra --> SQL
    Infra --> Redis
    Infra --> Rabbit
    Infra --> MinIO
    WebApi --> Seq
    WebApi --> Elastic
    Workers --> SQL
    Workers --> Rabbit
```

### نکات کلیدی
- پایگاه داده واحد است اما با schema-based modularization.
- کانال های async با Outbox/Inbox کنترل می شوند.
- مسیر logging هم داخلی (Logging module) و هم خارجی (Seq/ELK) پوشش داده شده است.

## 3) C4 - سطح Component (داخل Web API)

```mermaid
flowchart LR
    Endpoint["Endpoint Modules<br/>Users/Todos/Files/..."]
    Filters["Endpoint Filters<br/>Sanitization + Execution Logging"]
    Behaviors["Application Behaviors<br/>Validation + Logging Decorators"]
    Handlers[Command/Query Handlers]
    Domain[Domain Entities + Events]
    WriteDb[ApplicationDbContext]
    ReadDb[ApplicationReadDbContext]
    Outbox[Outbox/Inbox + Messaging]
    Workers[Background Workers]

    Endpoint --> Filters
    Filters --> Behaviors
    Behaviors --> Handlers
    Handlers --> Domain
    Handlers --> WriteDb
    Handlers --> ReadDb
    Domain --> Outbox
    Outbox --> Workers
```

## 4) سناریوهای مرجع (برای درک سریع)

### سناریو A: ورود کاربر
1. `POST /api/v1/users/login`
2. اعتبارسنجی credential + lockout checks
3. صدور access token و refresh token
4. ذخیره hash توکن refresh در `auth.RefreshTokens`

### سناریو B: اشتراک امن فایل
1. `GET /api/v1/files/{id}/link` برای ساخت لینک signed
2. `GET /api/v1/files/public/{token}` برای دسترسی anonymous کنترل شده
3. rate limit ویژه + audit log برای public access

### سناریو C: رخداد لاگ تا اعلان هشدار
1. ingest لاگ در `/api/v1/logging/events`
2. بررسی ruleهای alert
3. ایجاد `AlertIncident`
4. queue شدن اعلان برای ادمین ها (Notification module)

### سناریو D: بازیابی شکست های orchestration
1. مشاهده سلامت در `/api/v1/dashboard/orchestration-health`
2. replay outbox/inbox با endpointهای observability

## 5) مرزهای مسئولیت (Boundaries)
- Endpointها فقط ورودی/خروجی HTTP و orchestration لایه بالا را انجام می دهند.
- منطق use case در Handlerها و Serviceهای تعریف شده در قرارداد Application قرار دارد.
- زیرساخت قابل تعویض است (Redis, RabbitMQ, MinIO, ClamAV, sinks).

## 6) نکته معماری مهم
تمام endpointهای ماژولی زیر `/api/v1/*` map می شوند.
