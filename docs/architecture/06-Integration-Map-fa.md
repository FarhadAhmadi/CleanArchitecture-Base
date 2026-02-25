# Ù†Ù‚Ø´Ù‡ Integration Ø³ÛŒØ³ØªÙ…

ØªØ§Ø±ÛŒØ®: 2026-02-21  
Ù…Ø¨Ù†Ø§ÛŒ ØªØ­Ù„ÛŒÙ„: Ú©Ø¯ ÙØ¹Ù„ÛŒ `src/*` Ùˆ endpointÙ‡Ø§ÛŒ `src/Host/Endpoints/Modules/*`

## Ù‡Ø¯Ù
Ø§ÛŒÙ† Ø³Ù†Ø¯ Ù†Ù…Ø§ÛŒ ÛŒÚ©Ù¾Ø§Ø±Ú†Ù‡ ÙˆØ§Ø¨Ø³ØªÚ¯ÛŒ Ù…Ø§Ú˜ÙˆÙ„â€ŒÙ‡Ø§ØŒ IntegrationÙ‡Ø§ÛŒ Ø¯Ø§Ø®Ù„ÛŒ Ùˆ Ø³Ø±ÙˆÛŒØ³â€ŒÙ‡Ø§ÛŒ Ø¨ÛŒØ±ÙˆÙ†ÛŒ Ø±Ø§ Ø§Ø±Ø§Ø¦Ù‡ Ù…ÛŒâ€ŒØ¯Ù‡Ø¯ ØªØ§ Ø·Ø±Ø§Ø­ÛŒØŒ ØªÙˆØ³Ø¹Ù‡ Ùˆ Ø¹Ù…Ù„ÛŒØ§Øª Ø¨Ø§ Ø¯ÛŒØ¯ end-to-end Ø§Ù†Ø¬Ø§Ù… Ø´ÙˆØ¯.

## Integration Ø¯Ø§Ø®Ù„ÛŒ (Ù…Ø§Ú˜ÙˆÙ„ Ø¨Ù‡ Ù…Ø§Ú˜ÙˆÙ„)
| Ù…Ø¨Ø¯Ø§ | Ù…Ù‚ØµØ¯ | Ù†ÙˆØ¹ Ø§Ø±ØªØ¨Ø§Ø· | Ù‡Ø¯Ù |
|---|---|---|---|
| Users/Auth | Notifications | Ù‡Ù…Ú¯Ø§Ù… | Ø§Ø±Ø³Ø§Ù„ Ú©Ø¯ ØªØ§ÛŒÛŒØ¯ Ø§ÛŒÙ…ÛŒÙ„/Ø±ÛŒØ³Øª Ø±Ù…Ø² |
| Users/Auth | Audit | Ù‡Ù…Ú¯Ø§Ù… | Ø«Ø¨Øª Ø¹Ù…Ù„ÛŒØ§Øª Ø­Ø³Ø§Ø³ Ø§Ù…Ù†ÛŒØªÛŒ |
| Authorization | Audit | Ù‡Ù…Ú¯Ø§Ù… | Ø«Ø¨Øª ØªØºÛŒÛŒØ± Ù†Ù‚Ø´/Ù…Ø¬ÙˆØ² |
| Profiles | Files | Ù‡Ù…Ú¯Ø§Ù… | Ø§ØªØµØ§Ù„ avatar/music Ø¨Ù‡ ÙØ§ÛŒÙ„â€ŒÙ‡Ø§ |
| Profiles | Logging | Ø±ÙˆÛŒØ¯Ø§Ø¯ÛŒ | Ø«Ø¨Øª Ø±Ø®Ø¯Ø§Ø¯Ù‡Ø§ÛŒ ØªØºÛŒÛŒØ± Ù¾Ø±ÙˆÙØ§ÛŒÙ„ |
| Files | Logging | Ù‡Ù…Ú¯Ø§Ù…/Ø±ÙˆÛŒØ¯Ø§Ø¯ÛŒ | Ø«Ø¨Øª audit Ù„ÛŒÙ†Ú© Ø¹Ù…ÙˆÙ…ÛŒ Ùˆ Ø¯Ø³ØªØ±Ø³ÛŒâ€ŒÙ‡Ø§ |
| Notifications | Logging | Ø±ÙˆÛŒØ¯Ø§Ø¯ÛŒ | Ø«Ø¨Øª Ù†ØªÛŒØ¬Ù‡ Ø§Ø±Ø³Ø§Ù„ Ø§Ø¹Ù„Ø§Ù† |
| Logging | Notifications | Ù†Ø§Ù‡Ù…Ú¯Ø§Ù… | Ø§Ø±Ø³Ø§Ù„ Ù‡Ø´Ø¯Ø§Ø± incident |
| Observability | Integration | Ù‡Ù…Ú¯Ø§Ù… | replay inbox/outbox |
| Observability | Logging | Ù‡Ù…Ú¯Ø§Ù… | Ù†Ù…Ø§ÛŒØ´ KPI Ø³Ù„Ø§Ù…Øª |
| Scheduler | Notifications | Ù‡Ù…Ú¯Ø§Ù… | Ø§Ø¬Ø±Ø§ÛŒ jobÙ‡Ø§ÛŒ Ø§Ø¹Ù„Ø§Ù† (probe) |
| Scheduler | Logging | Ù‡Ù…Ú¯Ø§Ù… | Ø«Ø¨Øª Ø§Ø¬Ø±Ø§ØŒ Ø®Ø·Ø§ØŒ lag |
| Scheduler | Authorization | Ù‡Ù…Ú¯Ø§Ù… | Ú©Ù†ØªØ±Ù„ Ù…Ø¬ÙˆØ²Ù‡Ø§ÛŒ scheduler |
| Scheduler | Audit | Ù‡Ù…Ú¯Ø§Ù… | Ø±Ø¯Ú¯ÛŒØ±ÛŒ Ø¹Ù…Ù„ÛŒØ§Øª Ù…Ø¯ÛŒØ±ÛŒØªÛŒ |

## Integration Ø¨ÛŒØ±ÙˆÙ†ÛŒ
| Ø³Ø±ÙˆÛŒØ³ | Ù…Ø§Ú˜ÙˆÙ„(Ù‡Ø§) | Ú©Ø§Ø±Ø¨Ø±Ø¯ |
|---|---|---|
| SQL Server | Ù‡Ù…Ù‡ | persistence Ø¯Ø§Ø¯Ù‡ Ù…Ø§Ú˜ÙˆÙ„ÛŒ |
| Redis | Authorization, Caching | cache/versioning Ù…Ø¬ÙˆØ²Ù‡Ø§ |
| RabbitMQ | Integration, Notifications, Logging | Ù¾Ø±Ø¯Ø§Ø²Ø´ Ù†Ø§Ù‡Ù…Ú¯Ø§Ù… Ù¾ÛŒØ§Ù… |
| MinIO/Object Storage | Files | Ù†Ú¯Ù‡Ø¯Ø§Ø±ÛŒ ÙØ§ÛŒÙ„ |
| ClamAV (Ø§Ø®ØªÛŒØ§Ø±ÛŒ) | Files | Ø§Ø³Ú©Ù† Ø§Ù…Ù†ÛŒØªÛŒ ÙØ§ÛŒÙ„ |
| SMTP/SMS/Slack/Teams/Push Providers | Notifications | Ø§Ø±Ø³Ø§Ù„ Ú†Ù†Ø¯Ú©Ø§Ù†Ø§Ù„Ù‡ |
| OAuth Providers (Google/Meta) | Users/Auth | ÙˆØ±ÙˆØ¯ Ø¨Ø§ Ø­Ø³Ø§Ø¨ Ø¨ÛŒØ±ÙˆÙ†ÛŒ |
| Seq/Elastic/OTel | Logging/Observability | Ù¾Ø§ÛŒØ´ Ùˆ ØªØ­Ù„ÛŒÙ„ Ø¹Ù…Ù„ÛŒØ§ØªÛŒ |

## Ù†Ù…ÙˆØ¯Ø§Ø± Ú©Ù„Ø§Ù† Integration
```mermaid
flowchart LR
    U[Users/Auth] --> N[Notifications]
    U --> A[Audit]
    AZ[Authorization] --> A
    P[Profiles] --> F[Files]
    P --> L[Logging]
    F --> L
    N --> L
    L --> N
    O[Observability] --> I[Integration]
    O --> L
    S[Scheduler] --> N
    S --> L
    S --> AZ
    S --> A

    DB[(SQL Server)]
    R[(Redis)]
    MQ[(RabbitMQ)]
    M[(MinIO)]
    AV[(ClamAV)]
    OP[(OAuth Providers)]
    NP[(Notification Providers)]
    OB[(Seq/Elastic/OTel)]

    U --> DB
    AZ --> R
    I --> MQ
    N --> NP
    U --> OP
    F --> M
    F --> AV
    L --> OB
```

## Ø§Ù„Ú¯ÙˆÛŒ Ø§Ø³ØªØ§Ù†Ø¯Ø§Ø±Ø¯ Integration Ø¨Ø±Ø§ÛŒ Ù…Ø§Ú˜ÙˆÙ„ Ø¬Ø¯ÛŒØ¯
1. Ù‚Ø±Ø§Ø±Ø¯Ø§Ø¯ Ù…Ø§Ú˜ÙˆÙ„ (Command/Query/Event) Ø±Ø§ Ø¯Ø± Application ØªØ¹Ø±ÛŒÙ Ú©Ù†ÛŒØ¯.
2. persistence Ø±Ø§ Ø¯Ø± schema Ø§Ø®ØªØµØ§ØµÛŒ Ù…Ø§Ú˜ÙˆÙ„ Ù†Ú¯Ù‡ Ø¯Ø§Ø±ÛŒØ¯.
3. permission Ùˆ policy endpointÙ‡Ø§ Ø±Ø§ Ù‡Ù…â€ŒØ²Ù…Ø§Ù† Ø¨Ø§ API ØªØ¹Ø±ÛŒÙ Ú©Ù†ÛŒØ¯.
4. eventÙ‡Ø§ÛŒ Ø¨ÛŒÙ†â€ŒÙ…Ø§Ú˜ÙˆÙ„ÛŒ Ø±Ø§ Ø¨Ø§ Outbox Ù…Ù†ØªØ´Ø± Ú©Ù†ÛŒØ¯.
5. Ø³Ù†Ø¬Ù‡â€ŒÙ‡Ø§ÛŒ Ø³Ù„Ø§Ù…Øª (latency/error/retry) Ø±Ø§ Ø¯Ø± Observability Ù‚Ø§Ø¨Ù„ Ù…Ø´Ø§Ù‡Ø¯Ù‡ Ú©Ù†ÛŒØ¯.
6. Ø¹Ù…Ù„ÛŒØ§Øª Ø­Ø³Ø§Ø³ Ø±Ø§ Ø¯Ø± Audit Ø«Ø¨Øª Ú©Ù†ÛŒØ¯.

## Ú†Ú©â€ŒÙ„ÛŒØ³Øª Ø¨Ø§Ø²Ø¨ÛŒÙ†ÛŒ Integration
- Ù…Ø±Ø² schema Ù…Ø§Ú˜ÙˆÙ„ Ø±Ø¹Ø§ÛŒØª Ø´Ø¯Ù‡ Ø§Ø³Øª.
- dependency Ù…Ø³ØªÙ‚ÛŒÙ… Ø§Ø² Domain Ø¨Ù‡ Infrastructure ÙˆØ¬ÙˆØ¯ Ù†Ø¯Ø§Ø±Ø¯.
- Ù…Ø³ÛŒØ±Ù‡Ø§ÛŒ API Ø¯Ø§Ø±Ø§ÛŒ permission Ù…Ø´Ø®Øµ Ù‡Ø³ØªÙ†Ø¯.
- Ø®Ø·Ø§Ù‡Ø§ Ùˆ timeoutÙ‡Ø§ retry policy Ù…Ø´Ø®Øµ Ø¯Ø§Ø±Ù†Ø¯.
- Ø¨Ø±Ø§ÛŒ Ø¹Ù…Ù„ÛŒØ§Øª Ø¨Ø­Ø±Ø§Ù†ÛŒ runbook Ø¹Ù…Ù„ÛŒØ§ØªÛŒ ÙˆØ¬ÙˆØ¯ Ø¯Ø§Ø±Ø¯.

