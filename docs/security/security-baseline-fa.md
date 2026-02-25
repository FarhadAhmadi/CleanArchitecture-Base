# Security Baseline (Best Practice)

Ø§ÛŒÙ† Ø³Ù†Ø¯ ÙˆØ¶Ø¹ÛŒØª Ø§Ù…Ù†ÛŒØªÛŒ Ù¾Ø±ÙˆÚ˜Ù‡ Ø±Ø§ Ø¨Ù‡ Ø´Ú©Ù„ Ø§Ø¬Ø±Ø§ÛŒÛŒ Ù†Ú¯Ù‡ Ù…ÛŒâ€ŒØ¯Ø§Ø±Ø¯ Ùˆ Ù†Ø´Ø§Ù† Ù…ÛŒâ€ŒØ¯Ù‡Ø¯ Ù‡Ø± Ú©Ù†ØªØ±Ù„ Ø¯Ø± Ú©Ø¯ØŒ CI/CD ÛŒØ§ Ø¹Ù…Ù„ÛŒØ§Øª Ú†Ú¯ÙˆÙ†Ù‡ Ù¾ÙˆØ´Ø´ Ø¯Ø§Ø¯Ù‡ Ø´Ø¯Ù‡ Ø§Ø³Øª.

## 1) Ø§Ø­Ø±Ø§Ø² Ù‡ÙˆÛŒØªØŒ Ù†Ø´Ø³Øª Ùˆ Ø¯Ø³ØªØ±Ø³ÛŒ

- Ø§Ø­Ø±Ø§Ø² Ù‡ÙˆÛŒØª Ø§Ø³ØªØ§Ù†Ø¯Ø§Ø±Ø¯:
  - JWT + OAuth providerÙ‡Ø§ÛŒ Ø®Ø§Ø±Ø¬ÛŒ (`Google`/`Meta`) Ù¾ÛŒØ§Ø¯Ù‡ Ø³Ø§Ø²ÛŒ Ø´Ø¯Ù‡.
- Ø³ÛŒØ§Ø³Øª Ú¯Ø°Ø±ÙˆØ§Ú˜Ù‡ Ù‚ÙˆÛŒ:
  - Ø­Ø¯Ø§Ù‚Ù„ Ø·ÙˆÙ„ØŒ Ù¾ÛŒÚ†ÛŒØ¯Ú¯ÛŒØŒ Ø­Ø¯Ø§Ù‚Ù„ Ú©Ø§Ø±Ø§Ú©ØªØ± ÛŒÚ©ØªØ§ Ùˆ deny-list Ù¾Ø³ÙˆØ±Ø¯Ù‡Ø§ÛŒ Ø±Ø§ÛŒØ¬/Ù†Ø´ØªÛŒ Ø§Ø¹Ù…Ø§Ù„ Ù…ÛŒâ€ŒØ´ÙˆØ¯.
  - Ù…Ø³ÛŒØ±Ù‡Ø§: `src/Modules/Users/Infrastructure/Infrastructure/Authentication/PasswordPolicyValidator.cs`
- Ø¬Ù„ÙˆÚ¯ÛŒØ±ÛŒ Ø§Ø² reuse Ù¾Ø³ÙˆØ±Ø¯:
  - ØªØ§Ø±ÛŒØ®Ú†Ù‡ Ø±Ù…Ø² Ø¹Ø¨ÙˆØ± Ø¯Ø± Ø¯ÛŒØªØ§Ø¨ÛŒØ³ Ù†Ú¯Ù‡Ø¯Ø§Ø±ÛŒ Ùˆ enforce Ù…ÛŒâ€ŒØ´ÙˆØ¯.
  - Ù…Ø³ÛŒØ±Ù‡Ø§: `src/Domain/Modules/Users/UserPasswordHistory.cs`
- Ù…Ø¯ÛŒØ±ÛŒØª Ù†Ø´Ø³Øª:
  - refresh token rotation + reuse detection + revoke individual/all sessions.
  - reset password Ø¨Ø§Ø¹Ø« revoke ØªÙˆÚ©Ù†â€ŒÙ‡Ø§ÛŒ ÙØ¹Ø§Ù„ Ù…ÛŒâ€ŒØ´ÙˆØ¯.
- Ø§ØµÙ„ Ú©Ù…ØªØ±ÛŒÙ† Ø¯Ø³ØªØ±Ø³ÛŒ:
  - RBAC/permission-based authorization Ø¯Ø± Ú©Ù„ endpointÙ‡Ø§.

## 2) Ø§Ù…Ù†ÛŒØª API Ùˆ ÙˆØ±ÙˆØ¯ÛŒ/Ø®Ø±ÙˆØ¬ÛŒ

- Validation Ø³Ù…Øª Ø³Ø±ÙˆØ±:
  - FluentValidation + policyÙ‡Ø§ÛŒ Ù‚ÙˆÛŒ Ø¨Ø±Ø§ÛŒ Ø³Ù†Ø§Ø±ÛŒÙˆÙ‡Ø§ÛŒ auth.
- Hardening Ø¯Ø±Ø®ÙˆØ§Ø³Øª:
  - Ù…Ø­Ø¯ÙˆØ¯ÛŒØª Ø³Ø§ÛŒØ²/ØªØ¹Ø¯Ø§Ø¯ Ù‡Ø¯Ø±Ù‡Ø§ØŒ content-type enforcementØŒ reject Ù…ØªØ¯Ù‡Ø§ÛŒ TRACE/TRACK.
- Ø§Ù…Ù†ÛŒØª Ù¾Ø§Ø³Ø® HTTP:
  - CSP, X-Frame-Options, X-Content-Type-Options, Referrer-Policy, COOP/COEP/CORP.
- CORS:
  - fail-closed Ø¯Ø± production Ø¯Ø± ØµÙˆØ±Øª Ù†Ø¨ÙˆØ¯ origin Ù…Ø¬Ø§Ø².
- Rate limiting:
  - global + per-user/per-ip + policy Ø§Ø®ØªØµØ§ØµÛŒ Ù„ÛŒÙ†Ú© Ø¹Ù…ÙˆÙ…ÛŒ ÙØ§ÛŒÙ„.

## 3) Ø±Ù…Ø²Ù†Ú¯Ø§Ø±ÛŒØŒ Ø§Ø³Ø±Ø§Ø± Ùˆ Ú©Ù„ÛŒØ¯

- Ø±Ù…Ø²Ù†Ú¯Ø§Ø±ÛŒ Ø¯Ø± Ø§Ù†ØªÙ‚Ø§Ù„: TLS/HSTS Ø¯Ø± production.
- Ù…Ø¯ÛŒØ±ÛŒØª Ø§Ø³Ø±Ø§Ø±:
  - Azure Key Vault Ù¾Ø´ØªÛŒØ¨Ø§Ù†ÛŒ Ù…ÛŒâ€ŒØ´ÙˆØ¯.
  - Ø§Ø¹ØªØ¨Ø§Ø±Ø³Ù†Ø¬ÛŒ startup Ø¨Ø±Ø§ÛŒ Ø¬Ù„ÙˆÚ¯ÛŒØ±ÛŒ Ø§Ø² placeholder secret Ø¯Ø± Ù…Ø­ÛŒØ· ØºÛŒØ±-development ÙØ¹Ø§Ù„ Ø§Ø³Øª.
- ØªÙˆÙ„ÛŒØ¯ ØªØµØ§Ø¯ÙÛŒ Ø§Ù…Ù†:
  - `RandomNumberGenerator` Ø¨Ø±Ø§ÛŒ token/code Ø§Ø³ØªÙØ§Ø¯Ù‡ Ø´Ø¯Ù‡ Ø§Ø³Øª.

## 4) Ù„Ø§Ú¯ÛŒÙ†Ú¯ØŒ Ù…Ø§Ù†ÛŒØªÙˆØ±ÛŒÙ†Ú¯ Ùˆ Ù¾Ø§Ø³Ø® Ø­Ø§Ø¯Ø«Ù‡

- Ù„Ø§Ú¯ Ø§Ù…Ù†ÛŒØªÛŒ:
  - Ø±ÙˆÛŒØ¯Ø§Ø¯Ù‡Ø§ÛŒ auth/authz Ùˆ account lock Ø«Ø¨Øª Ù…ÛŒâ€ŒØ´ÙˆØ¯.
- Ø¹Ø¯Ù… Ù†Ø´Øª Ø¯Ø§Ø¯Ù‡ Ø­Ø³Ø§Ø³ Ø¯Ø± Ù„Ø§Ú¯:
  - query string Ù‚Ø¨Ù„ Ø§Ø² log redaction Ù…ÛŒâ€ŒØ´ÙˆØ¯ (token/password/code/...).
- runbookÙ‡Ø§:
  - incident/dr/backup runbookÙ‡Ø§ Ù…ÙˆØ¬ÙˆØ¯ Ù‡Ø³ØªÙ†Ø¯.

## 5) Ø§Ù…Ù†ÛŒØª Ø²Ù†Ø¬ÛŒØ±Ù‡ ØªØ§Ù…ÛŒÙ† Ùˆ CI/CD

- SAST: CodeQL
- SCA/Dependency scan: NuGet vulnerability + Trivy
- Secret scanning: Gitleaks
- DAST (API baseline): OWASP ZAP API scan Ø±ÙˆÛŒ OpenAPI snapshot
- Dependabot: Ø¨Ø±ÙˆØ²Ø±Ø³Ø§Ù†ÛŒ Ø±ÙˆØ²Ø§Ù†Ù‡ NuGet

## 6) VDP / Ú¯Ø²Ø§Ø±Ø´ Ø¢Ø³ÛŒØ¨â€ŒÙ¾Ø°ÛŒØ±ÛŒ

- Ø³ÛŒØ§Ø³Øª Ú¯Ø²Ø§Ø±Ø´ Ø¢Ø³ÛŒØ¨â€ŒÙ¾Ø°ÛŒØ±ÛŒ Ø¯Ø± `SECURITY.md` ØªØ¹Ø±ÛŒÙ Ø´Ø¯Ù‡ Ø§Ø³Øª.

## 7) Ù…ÙˆØ§Ø±Ø¯ Ø®Ø§Ø±Ø¬ Ø§Ø² scope Ù…Ø³ØªÙ‚ÛŒÙ… Ú©Ø¯

Ù…ÙˆØ§Ø±Ø¯ Ø²ÛŒØ± Ù†ÛŒØ§Ø²Ù…Ù†Ø¯ Ú©Ù†ØªØ±Ù„ Ø³Ø·Ø­ Ø²ÛŒØ±Ø³Ø§Ø®Øª/Ø³Ø§Ø²Ù…Ø§Ù† Ø§Ø³Øª Ùˆ Ø¨Ø§ÛŒØ¯ Ø¯Ø± Ù…Ø­ÛŒØ· Ø¹Ù…Ù„ÛŒØ§ØªÛŒ enforce Ø´ÙˆØ¯:

- WAF, network segmentation, host hardening
- mTLS service-to-service
- periodic pentest
- backup encryption verification Ùˆ drillÙ‡Ø§ÛŒ Ø¯ÙˆØ±Ù‡â€ŒØ§ÛŒ
- secure mobile token storage / anti-reverse-engineering
- secure coding training Ø¨Ø±Ù†Ø§Ù…Ù‡â€ŒØ¯Ø§Ø±
- data classification, retention, legal compliance (Ù…Ø§Ù†Ù†Ø¯ GDPR Ø¯Ø± ØµÙˆØ±Øª Ù†ÛŒØ§Ø²)

