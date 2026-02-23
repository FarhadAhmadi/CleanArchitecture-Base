## Summary
- What changed:
- Why this change is needed:
- Risk level (`low` / `medium` / `high`):

## Quality Gate Checklist
- [ ] `dotnet build CleanArchitecture.slnx -c Release` passed
- [ ] `dotnet test tests/ArchitectureTests/ArchitectureTests.csproj -c Release` passed
- [ ] `dotnet test tests/ContractTests/ContractTests.csproj -c Release` passed (or snapshot updated intentionally)
- [ ] No pending model changes (`dotnet ef migrations has-pending-model-changes`)
- [ ] New/changed behavior has test coverage

## Security Checklist
- [ ] No secrets committed
- [ ] Dependency vulnerabilities reviewed
- [ ] Threat/risk notes added for security-sensitive changes

## Operational Checklist
- [ ] SLO impact evaluated (availability, latency, 5xx)
- [ ] Observability updated (logs/metrics/traces/alerts) if needed
- [ ] Runbook/docs updated if behavior changed

## Database / Migration
- [ ] Schema change included (if required)
- [ ] Backward compatibility considered
- [ ] Rollback strategy documented

## Deployment Notes
- Config changes:
- Feature flag / kill switch:
- Post-deploy checks:
