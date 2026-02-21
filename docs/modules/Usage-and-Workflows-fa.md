# روند استفاده و Workflow ماژول‌ها

تاریخ: 2026-02-21

## 1) Users/Auth
### Integrationها
- Notification برای OTP/Verification
- Audit برای رخدادهای امنیتی

### روند استفاده
1. کاربر `register` می‌کند.
2. کد تایید ایمیل ارسال می‌شود.
3. کاربر تایید ایمیل انجام می‌دهد.
4. کاربر `login` می‌کند و access/refresh می‌گیرد.
5. در انقضا، `refresh` انجام می‌شود.

### Workflow
```mermaid
sequenceDiagram
    actor User
    participant API as Users API
    participant N as Notifications
    participant DB as SQL
    User->>API: POST /users/register
    API->>DB: Create User
    API->>N: Send verification code
    User->>API: POST /users/verify-email
    API->>DB: Mark EmailConfirmed
    User->>API: POST /users/login
    API-->>User: Access + Refresh Token
```

## 2) Authorization
### Integrationها
- Caching (Redis/Memory) برای versioning permission
- Audit برای ثبت تغییرات کنترل دسترسی

### روند استفاده
1. ادمین نقش را به کاربر اختصاص می‌دهد.
2. ادمین مجوز را به نقش اختصاص می‌دهد.
3. APIها با policy پویا مجوز را enforce می‌کنند.

### Workflow
```mermaid
flowchart LR
    A[Admin] --> B[Assign Role]
    B --> C[Assign Permission]
    C --> D[Update Permission Cache Version]
    D --> E[Policy Check on API]
    E --> F[Allow/Deny]
```

## 3) Todos
### Integrationها
- وابستگی مستقیم به user context

### روند استفاده
1. کاربر todo ایجاد می‌کند.
2. لیست todo را با paging می‌بیند.
3. مورد را complete یا delete می‌کند.

### Workflow
```mermaid
flowchart TD
    U[User] --> C[Create Todo]
    C --> L[List Todos]
    L --> X{Action}
    X -->|Complete| M[Mark Completed]
    X -->|Delete| D[Delete Todo]
```

## 4) Profiles
### Integrationها
- Files برای avatar/music
- Logging/Notifications برای رویداد تغییرات مهم

### روند استفاده
1. کاربر پروفایل خود را می‌سازد.
2. بخش‌های مختلف را patch می‌کند.
3. public profile برای نمایش عمومی خوانده می‌شود.

### Workflow
```mermaid
sequenceDiagram
    actor User
    participant API as Profiles API
    participant F as Files
    participant DB as SQL
    User->>API: POST /profiles/me
    API->>DB: Create Profile
    User->>API: PUT /profiles/me/avatar
    API->>F: Validate File Ownership
    API->>DB: Save AvatarFileId
    User->>API: GET /profiles/{id}/public
    API-->>User: Public Profile
```

## 5) Files
### Integrationها
- MinIO برای object storage
- ClamAV (اختیاری) برای scan
- Logging برای audit دسترسی عمومی

### روند استفاده
1. کاربر فایل upload می‌کند.
2. validate/scan انجام می‌شود.
3. metadata ثبت و فایل قابل دانلود/stream می‌شود.
4. برای اشتراک، secure/public link تولید می‌شود.

### Workflow
```mermaid
flowchart LR
    U[User] --> UP[Upload]
    UP --> V[Validate]
    V --> S[Scan]
    S --> M[Save Metadata]
    M --> L[Generate Link]
    L --> A[Access with Token]
    A --> G[Audit Access]
```

## 6) Notifications
### Integrationها
- Providerهای بیرونی ارسال
- Logging برای ثبت نتیجه ارسال

### روند استفاده
1. اعلان با template یا payload خام ساخته می‌شود.
2. در صورت نیاز schedule ثبت می‌شود.
3. worker اعلان را dispatch می‌کند.
4. نتیجه در attempts ذخیره می‌شود.

### Workflow
```mermaid
sequenceDiagram
    actor Admin
    participant API as Notifications API
    participant W as Dispatch Worker
    participant P as Provider
    participant DB as SQL
    Admin->>API: POST /notifications
    API->>DB: Save Pending Message
    API->>DB: Optional Schedule
    W->>DB: Pull due messages
    W->>P: Send
    P-->>W: Delivery result
    W->>DB: Save attempt/status
```

## 7) Logging
### Integrationها
- مصرف‌کننده رخداد از ماژول‌های مختلف
- اتصال به Notifications برای incident alert

### روند استفاده
1. رخداد single/bulk ingest می‌شود.
2. validate/transform انجام می‌شود.
3. ruleها incident تولید می‌کنند.
4. incident قابل query و پیگیری است.

### Workflow
```mermaid
flowchart TD
    I[Ingest Event] --> V[Validate Schema]
    V --> T[Transform]
    T --> W[Write LogEvent]
    W --> R{Rule Match?}
    R -->|Yes| N[Create Incident]
    R -->|No| E[End]
    N --> A[Notify Admin]
```

## 8) Audit
### Integrationها
- دریافت عملیات حساس از ماژول‌های مختلف

### روند استفاده
1. عملیات حساس ثبت می‌شود.
2. checksum chain کامل می‌شود.
3. گزارش integrity به‌صورت دوره‌ای بررسی می‌شود.

### Workflow
```mermaid
flowchart LR
    O[Sensitive Operation] --> H[Hash Payload]
    H --> C[Link with Previous Checksum]
    C --> S[Store AuditEntry]
    S --> R[Integrity Report]
```

## 9) Observability
### Integrationها
- Integration (outbox/inbox)
- Logging و Notifications برای KPI

### روند استفاده
1. تیم عملیات متریک‌ها و health را مانیتور می‌کند.
2. خطاهای inbox/outbox را replay می‌کند.
3. نتیجه replay را ارزیابی می‌کند.

### Workflow
```mermaid
sequenceDiagram
    actor Ops
    participant O as Observability API
    participant I as Integration
    Ops->>O: GET /observability/metrics
    O-->>Ops: KPI Snapshot
    Ops->>O: POST /observability/orchestration/replay/outbox
    O->>I: Replay failed messages
    I-->>O: Replay result
    O-->>Ops: Summary
```

## 10) Scheduler
### Integrationها
- Notifications (jobهای اعلان)
- Logging/Metrics برای پایش اجرا
- Authorization/Audit برای کنترل و ردیابی

### روند استفاده
1. Job ایجاد می‌شود.
2. Schedule تعریف می‌شود.
3. Worker due job را پیدا می‌کند.
4. lock گرفته می‌شود و handler اجرا می‌شود.
5. نتیجه اجرا/Retry/Quarantine ثبت می‌شود.

### Workflow
```mermaid
flowchart TD
    J[Create Job] --> S[Set Schedule]
    S --> D[Worker Detect Due Job]
    D --> L[Acquire Distributed Lock]
    L --> X[Execute Handler]
    X --> R{Result}
    R -->|Success| N[Compute NextRun]
    R -->|Fail & Retry| B[Backoff + Retry]
    R -->|Repeated Fail| Q[Quarantine Job]
    N --> E[Write Execution Log]
    B --> E
    Q --> E
```

## 11) Shared Platform
### Integrationها
- DataAccess/Caching/Integration/HealthChecks برای همه ماژول‌ها

### روند استفاده
1. هر ماژول از abstractionهای Application استفاده می‌کند.
2. پیاده‌سازی‌های Infrastructure تزریق می‌شوند.
3. health و retry به‌صورت مرکزی اعمال می‌شود.

### Workflow
```mermaid
flowchart LR
    A[Application Module] --> AB[Abstractions]
    AB --> INF[Infrastructure Implementations]
    INF --> DB[(SQL)]
    INF --> MQ[(RabbitMQ)]
    INF --> R[(Redis)]
    INF --> H[HealthChecks/Resilience]
```
