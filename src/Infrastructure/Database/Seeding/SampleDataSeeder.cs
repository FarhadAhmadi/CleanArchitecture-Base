using System.Security.Cryptography;
using System.Text;
using Domain.Auditing;
using Domain.Authorization;
using Domain.Files;
using Domain.Logging;
using Domain.Modules.Notifications;
using Domain.Modules.Scheduler;
using Domain.Notifications;
using Domain.Profiles;
using Domain.Scheduler;
using Domain.Todos;
using Domain.Users;
using Infrastructure.Database;
using Infrastructure.Notifications;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Seeding;

public sealed class SampleDataSeeder(
    ApplicationDbContext dbContext,
    UserManager<User> userManager)
{
    private const string AdminEmail = "admin.sample@cleanarch.local";
    private const string OpsEmail = "ops.sample@cleanarch.local";
    private const string SupportEmail = "support.sample@cleanarch.local";
    private const string CustomerEmail = "customer.sample@cleanarch.local";
    private const string DefaultPassword = "P@ssw0rd123!";

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        DateTime now = DateTime.UtcNow;

        User admin = await EnsureUserAsync(AdminEmail, "System", "Admin", cancellationToken);
        User ops = await EnsureUserAsync(OpsEmail, "Ops", "Engineer", cancellationToken);
        User support = await EnsureUserAsync(SupportEmail, "Support", "Agent", cancellationToken);
        User customer = await EnsureUserAsync(CustomerEmail, "Test", "Customer", cancellationToken);

        await SeedUserRolesAsync(admin, ops, support, customer, cancellationToken);
        await SeedUserPermissionsAsync(ops.Id, cancellationToken);
        await SeedUserExternalLoginsAsync(support.Id, cancellationToken);
        await SeedRefreshTokensAsync(admin.Id, customer.Id, now, cancellationToken);
        await SeedTodosAsync(admin.Id, ops.Id, customer.Id, now, cancellationToken);
        await SeedProfilesAsync(admin, ops, support, customer, now, cancellationToken);

        (Guid specFileId, Guid imageFileId) = await SeedFilesAsync(admin.Id, ops.Id, customer.Id, now, cancellationToken);
        await SeedFileTagsAsync(specFileId, imageFileId, cancellationToken);
        await SeedFilePermissionsAsync(specFileId, customer.Id, cancellationToken);
        await SeedFileAuditsAsync(specFileId, imageFileId, admin.Id, customer.Id, now, cancellationToken);

        (Guid warnLogId, Guid errorLogId) = await SeedLogEventsAsync(admin.Id, ops.Id, now, cancellationToken);
        Guid ruleId = await SeedAlertRuleAsync(cancellationToken);
        await SeedAlertIncidentAsync(ruleId, errorLogId, now, cancellationToken);

        Guid notificationId = await SeedNotificationsAsync(admin.Id, customer.Id, now, cancellationToken);
        await SeedNotificationPermissionsAsync(notificationId, customer.Id, cancellationToken);
        await SeedNotificationScheduleAsync(notificationId, now, cancellationToken);
        await SeedNotificationDeliveryAttemptsAsync(notificationId, now, cancellationToken);

        await SeedSchedulerRuntimeAsync(ops.Id, now, cancellationToken);
        await SeedAuditTrailAsync(admin.Id, ops.Id, notificationId, warnLogId, now, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<User> EnsureUserAsync(
        string email,
        string firstName,
        string lastName,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        User? user = await userManager.FindByEmailAsync(email);
        if (user is not null)
        {
            bool changed = false;
            if (!string.Equals(user.FirstName, firstName, StringComparison.Ordinal))
            {
                user.FirstName = firstName;
                changed = true;
            }

            if (!string.Equals(user.LastName, lastName, StringComparison.Ordinal))
            {
                user.LastName = lastName;
                changed = true;
            }

            if (!user.EmailConfirmed)
            {
                user.EmailConfirmed = true;
                changed = true;
            }

            if (changed)
            {
                await userManager.UpdateAsync(user);
            }

            return user;
        }

        user = new User
        {
            Id = Guid.NewGuid(),
            UserName = email,
            NormalizedUserName = email.ToUpperInvariant(),
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            EmailConfirmed = true,
            FirstName = firstName,
            LastName = lastName,
            LockoutEnabled = true,
            SecurityStamp = Guid.NewGuid().ToString("N")
        };

        IdentityResult createResult = await userManager.CreateAsync(user, DefaultPassword);
        if (!createResult.Succeeded)
        {
            throw new InvalidOperationException($"Failed to create sample user '{email}'.");
        }

        return user;
    }

    private async Task SeedUserRolesAsync(
        User admin,
        User ops,
        User support,
        User customer,
        CancellationToken cancellationToken)
    {
        Dictionary<string, Guid> roles = await dbContext.Roles
            .Where(r =>
                r.Name == "admin" ||
                r.Name == "user" ||
                r.Name == "LogReader" ||
                r.Name == "SecurityAnalyst")
            .ToDictionaryAsync(r => r.Name!, r => r.Id, StringComparer.OrdinalIgnoreCase, cancellationToken);

        List<UserRole> assignments = [];

        if (roles.TryGetValue("admin", out Guid adminRoleId))
        {
            assignments.Add(new UserRole { UserId = admin.Id, RoleId = adminRoleId });
        }

        if (roles.TryGetValue("user", out Guid userRoleId))
        {
            assignments.Add(new UserRole { UserId = admin.Id, RoleId = userRoleId });
            assignments.Add(new UserRole { UserId = ops.Id, RoleId = userRoleId });
            assignments.Add(new UserRole { UserId = support.Id, RoleId = userRoleId });
            assignments.Add(new UserRole { UserId = customer.Id, RoleId = userRoleId });
        }

        if (roles.TryGetValue("LogReader", out Guid logReaderRoleId))
        {
            assignments.Add(new UserRole { UserId = ops.Id, RoleId = logReaderRoleId });
        }

        if (roles.TryGetValue("SecurityAnalyst", out Guid securityRoleId))
        {
            assignments.Add(new UserRole { UserId = support.Id, RoleId = securityRoleId });
        }

        HashSet<string> existing = [.. await dbContext.UserRoles
            .Select(x => $"{x.UserId}:{x.RoleId}")
            .ToListAsync(cancellationToken)];

        foreach (UserRole assignment in assignments)
        {
            if (existing.Add($"{assignment.UserId}:{assignment.RoleId}"))
            {
                dbContext.UserRoles.Add(assignment);
            }
        }
    }

    private async Task SeedUserPermissionsAsync(Guid opsUserId, CancellationToken cancellationToken)
    {
        Dictionary<string, Guid> permissionIds = await dbContext.Permissions
            .Where(x =>
                x.Code == PermissionCodes.ObservabilityManage ||
                x.Code == PermissionCodes.NotificationReportsRead)
            .ToDictionaryAsync(x => x.Code, x => x.Id, StringComparer.OrdinalIgnoreCase, cancellationToken);

        if (permissionIds.Count == 0)
        {
            return;
        }

        HashSet<string> existing = [.. await dbContext.UserPermissions
            .Select(x => $"{x.UserId}:{x.PermissionId}")
            .ToListAsync(cancellationToken)];

        foreach (Guid permissionId in permissionIds.Values)
        {
            string key = $"{opsUserId}:{permissionId}";
            if (existing.Add(key))
            {
                dbContext.UserPermissions.Add(new UserPermission
                {
                    UserId = opsUserId,
                    PermissionId = permissionId
                });
            }
        }
    }

    private async Task SeedUserExternalLoginsAsync(Guid supportUserId, CancellationToken cancellationToken)
    {
        bool exists = await dbContext.UserExternalLogins
            .AnyAsync(x => x.Provider == "google" && x.ProviderUserId == "support-seed-google", cancellationToken);

        if (exists)
        {
            return;
        }

        dbContext.UserExternalLogins.Add(new UserExternalLogin
        {
            Id = Guid.Parse("E6A6A54D-90A6-4DF5-8D58-955B7E847501"),
            UserId = supportUserId,
            Provider = "google",
            ProviderUserId = "support-seed-google",
            Email = SupportEmail,
            LinkedAtUtc = DateTime.UtcNow.AddDays(-10)
        });
    }

    private async Task SeedRefreshTokensAsync(
        Guid adminUserId,
        Guid customerUserId,
        DateTime now,
        CancellationToken cancellationToken)
    {
        string adminTokenHash = ComputeSha256("seed-admin-refresh-token");
        string customerTokenHash = ComputeSha256("seed-customer-refresh-token");

        if (!await dbContext.RefreshTokens.AnyAsync(x => x.TokenHash == adminTokenHash, cancellationToken))
        {
            dbContext.RefreshTokens.Add(new RefreshToken
            {
                Id = Guid.Parse("03F59E8A-1D57-4E04-85A6-1553D9620A21"),
                UserId = adminUserId,
                TokenHash = adminTokenHash,
                CreatedAtUtc = now.AddHours(-3),
                ExpiresAtUtc = now.AddDays(10)
            });
        }

        if (!await dbContext.RefreshTokens.AnyAsync(x => x.TokenHash == customerTokenHash, cancellationToken))
        {
            dbContext.RefreshTokens.Add(new RefreshToken
            {
                Id = Guid.Parse("5CE4C500-6992-4D7E-BBDF-2E2A29129E95"),
                UserId = customerUserId,
                TokenHash = customerTokenHash,
                CreatedAtUtc = now.AddHours(-2),
                ExpiresAtUtc = now.AddDays(7)
            });
        }
    }

    private async Task SeedTodosAsync(
        Guid adminUserId,
        Guid opsUserId,
        Guid customerUserId,
        DateTime now,
        CancellationToken cancellationToken)
    {
        List<TodoItem> todos =
        [
            new TodoItem
            {
                Id = Guid.Parse("8665934A-47C9-43D8-9DB4-32D4B67CA5A1"),
                UserId = adminUserId,
                Description = "Review weekly operational metrics and close high-risk alerts.",
                Priority = Priority.High,
                Labels = ["ops", "weekly"],
                IsCompleted = false,
                CreatedAt = now.AddDays(-4),
                DueDate = now.AddDays(1)
            },
            new TodoItem
            {
                Id = Guid.Parse("B447A932-37F9-4864-8687-D058E3396A6A"),
                UserId = opsUserId,
                Description = "Validate scheduler dead-letter replay flow in staging.",
                Priority = Priority.Top,
                Labels = ["scheduler", "staging"],
                IsCompleted = true,
                CreatedAt = now.AddDays(-3),
                DueDate = now.AddDays(-1),
                CompletedAt = now.AddDays(-1)
            },
            new TodoItem
            {
                Id = Guid.Parse("82C295A2-B866-47C9-8998-458D12D94EB9"),
                UserId = customerUserId,
                Description = "Upload onboarding documents and share with support team.",
                Priority = Priority.Medium,
                Labels = ["files", "onboarding"],
                IsCompleted = false,
                CreatedAt = now.AddDays(-2),
                DueDate = now.AddDays(2)
            }
        ];

        HashSet<Guid> existingIds = [.. await dbContext.TodoItems.Select(x => x.Id).ToListAsync(cancellationToken)];
        foreach (TodoItem todo in todos.Where(x => existingIds.Add(x.Id)))
        {
            dbContext.TodoItems.Add(todo);
        }
    }

    private async Task SeedProfilesAsync(
        User admin,
        User ops,
        User support,
        User customer,
        DateTime now,
        CancellationToken cancellationToken)
    {
        UserProfile[] profiles =
        [
            BuildProfile(admin.Id, "System Admin", "Tehran", "Asia/Tehran", "DevOps,Observability,Security", true, now),
            BuildProfile(ops.Id, "Ops Engineer", "Shiraz", "Asia/Tehran", "SRE,Scheduler,Reliability", true, now),
            BuildProfile(support.Id, "Support Agent", "Tabriz", "Asia/Tehran", "Support,Files,Notifications", false, now),
            BuildProfile(customer.Id, "Test Customer", "Mashhad", "Asia/Tehran", "Onboarding,Profile,Todos", true, now)
        ];

        HashSet<Guid> existingUserIds = [.. await dbContext.UserProfiles
            .Select(x => x.UserId)
            .ToListAsync(cancellationToken)];

        foreach (UserProfile profile in profiles.Where(x => existingUserIds.Add(x.UserId)))
        {
            dbContext.UserProfiles.Add(profile);
        }
    }

    private async Task<(Guid specFileId, Guid imageFileId)> SeedFilesAsync(
        Guid adminUserId,
        Guid opsUserId,
        Guid customerUserId,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var specFileId = Guid.Parse("76D91F26-B00D-412D-B5B2-9E2DDAFA4D6D");
        var imageFileId = Guid.Parse("66B9CD56-B678-466F-8E24-26B489F2F389");

        FileAsset[] files =
        [
            new FileAsset
            {
                Id = specFileId,
                OwnerUserId = adminUserId,
                ObjectKey = "seed/docs/architecture-v1.pdf",
                FileName = "architecture-v1.pdf",
                Extension = ".pdf",
                ContentType = "application/pdf",
                SizeBytes = 1_248_882,
                Module = "docs",
                Folder = "architecture",
                Description = "Seeded architecture baseline document.",
                Sha256 = ComputeSha256("seed-architecture-v1"),
                IsScanned = true,
                IsInfected = false,
                IsEncrypted = false,
                UploadedAtUtc = now.AddDays(-15)
            },
            new FileAsset
            {
                Id = imageFileId,
                OwnerUserId = customerUserId,
                ObjectKey = "seed/avatars/customer-main.png",
                FileName = "customer-main.png",
                Extension = ".png",
                ContentType = "image/png",
                SizeBytes = 381_440,
                Module = "profiles",
                Folder = "avatars",
                Description = "Seeded profile avatar.",
                Sha256 = ComputeSha256("seed-customer-avatar"),
                IsScanned = true,
                IsInfected = false,
                IsEncrypted = true,
                UploadedAtUtc = now.AddDays(-6),
                UpdatedAtUtc = now.AddDays(-2)
            },
            new FileAsset
            {
                Id = Guid.Parse("497F31D3-8A69-4A71-88B6-B21222374A8E"),
                OwnerUserId = opsUserId,
                ObjectKey = "seed/reports/scheduler-health.csv",
                FileName = "scheduler-health.csv",
                Extension = ".csv",
                ContentType = "text/csv",
                SizeBytes = 92_301,
                Module = "scheduler",
                Folder = "reports",
                Description = "Seeded scheduler report export.",
                Sha256 = ComputeSha256("seed-scheduler-csv"),
                IsScanned = true,
                IsInfected = false,
                IsEncrypted = false,
                UploadedAtUtc = now.AddDays(-3)
            }
        ];

        HashSet<Guid> existing = [.. await dbContext.FileAssets.Select(x => x.Id).ToListAsync(cancellationToken)];
        foreach (FileAsset file in files.Where(x => existing.Add(x.Id)))
        {
            dbContext.FileAssets.Add(file);
        }

        return (specFileId, imageFileId);
    }

    private async Task SeedFileTagsAsync(Guid specFileId, Guid imageFileId, CancellationToken cancellationToken)
    {
        List<FileTag> tags =
        [
            new FileTag { FileId = specFileId, Tag = "architecture" },
            new FileTag { FileId = specFileId, Tag = "baseline" },
            new FileTag { FileId = imageFileId, Tag = "avatar" },
            new FileTag { FileId = imageFileId, Tag = "profile" }
        ];

        HashSet<string> existing = [.. await dbContext.FileTags
            .Select(x => $"{x.FileId}:{x.Tag}")
            .ToListAsync(cancellationToken)];

        foreach (FileTag tag in tags)
        {
            string key = $"{tag.FileId}:{tag.Tag}";
            if (existing.Add(key))
            {
                dbContext.FileTags.Add(tag);
            }
        }
    }

    private async Task SeedFilePermissionsAsync(Guid specFileId, Guid customerUserId, CancellationToken cancellationToken)
    {
        List<FilePermissionEntry> entries =
        [
            new FilePermissionEntry
            {
                Id = Guid.Parse("EA9D3CF2-8ADA-4428-9CD6-5A3500307188"),
                FileId = specFileId,
                SubjectType = "role",
                SubjectValue = "admin",
                CanRead = true,
                CanWrite = true,
                CanDelete = true,
                CreatedAtUtc = DateTime.UtcNow.AddDays(-14)
            },
            new FilePermissionEntry
            {
                Id = Guid.Parse("61ECEB4E-7B7A-4284-A2AA-CBA4D3EC74F1"),
                FileId = specFileId,
                SubjectType = "user",
                SubjectValue = customerUserId.ToString("N"),
                CanRead = true,
                CanWrite = false,
                CanDelete = false,
                CreatedAtUtc = DateTime.UtcNow.AddDays(-12)
            }
        ];

        HashSet<string> existing = [.. await dbContext.FilePermissionEntries
            .Select(x => $"{x.FileId}:{x.SubjectType}:{x.SubjectValue}")
            .ToListAsync(cancellationToken)];

        foreach (FilePermissionEntry entry in entries)
        {
            string key = $"{entry.FileId}:{entry.SubjectType}:{entry.SubjectValue}";
            if (existing.Add(key))
            {
                dbContext.FilePermissionEntries.Add(entry);
            }
        }
    }

    private async Task SeedFileAuditsAsync(
        Guid specFileId,
        Guid imageFileId,
        Guid adminUserId,
        Guid customerUserId,
        DateTime now,
        CancellationToken cancellationToken)
    {
        List<FileAccessAudit> audits =
        [
            new FileAccessAudit
            {
                Id = Guid.Parse("CFAEB1E0-0208-4E14-BA2D-985DD0BB5418"),
                FileId = specFileId,
                UserId = adminUserId,
                Action = "download",
                TimestampUtc = now.AddDays(-4),
                IpAddress = "10.10.1.20",
                UserAgent = "SeedClient/1.0"
            },
            new FileAccessAudit
            {
                Id = Guid.Parse("114BA6DD-6EF1-4A99-8CCE-D09811915E22"),
                FileId = imageFileId,
                UserId = customerUserId,
                Action = "preview",
                TimestampUtc = now.AddDays(-1),
                IpAddress = "10.10.1.21",
                UserAgent = "SeedClient/1.0"
            }
        ];

        HashSet<Guid> existing = [.. await dbContext.FileAccessAudits.Select(x => x.Id).ToListAsync(cancellationToken)];
        foreach (FileAccessAudit audit in audits.Where(x => existing.Add(x.Id)))
        {
            dbContext.FileAccessAudits.Add(audit);
        }
    }

    private async Task<(Guid warnLogId, Guid errorLogId)> SeedLogEventsAsync(
        Guid adminUserId,
        Guid opsUserId,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var warnLogId = Guid.Parse("127A2E08-CC3A-4EBC-AD3D-C64AF32DB5AC");
        var errorLogId = Guid.Parse("FCDBF0E2-EF8A-4F6B-A7C7-23B49A3DF22A");

        LogEvent[] logs =
        [
            BuildLogEvent(
                warnLogId,
                "seed-log-warn-01",
                now.AddMinutes(-35),
                LogLevelType.Warning,
                "Scheduler delayed execution detected for one job.",
                "scheduler",
                "runtime",
                opsUserId.ToString("N"),
                "degraded"),
            BuildLogEvent(
                errorLogId,
                "seed-log-error-01",
                now.AddMinutes(-18),
                LogLevelType.Error,
                "Notification dispatch failed after max retries.",
                "notifications",
                "dispatch",
                adminUserId.ToString("N"),
                "failed")
        ];

        HashSet<string> existingKeys = [.. await dbContext.LogEvents
            .Where(x => x.IdempotencyKey != null)
            .Select(x => x.IdempotencyKey!)
            .ToListAsync(cancellationToken)];

        foreach (LogEvent log in logs)
        {
            if (log.IdempotencyKey is null || existingKeys.Add(log.IdempotencyKey))
            {
                bool hasId = await dbContext.LogEvents.AnyAsync(x => x.Id == log.Id, cancellationToken);
                if (!hasId)
                {
                    dbContext.LogEvents.Add(log);
                }
            }
        }

        return (warnLogId, errorLogId);
    }

    private async Task<Guid> SeedAlertRuleAsync(CancellationToken cancellationToken)
    {
        AlertRule? existing = await dbContext.AlertRules
            .SingleOrDefaultAsync(x => x.Name == "Seed Notification Failure Spike", cancellationToken);

        if (existing is not null)
        {
            return existing.Id;
        }

        AlertRule rule = new()
        {
            Id = Guid.Parse("D9D6F0F0-4A93-4801-95DC-BD04BC0FD1A8"),
            Name = "Seed Notification Failure Spike",
            IsEnabled = true,
            MinimumLevel = LogLevelType.Error,
            ContainsText = "dispatch failed",
            WindowSeconds = 300,
            ThresholdCount = 2,
            Action = "email",
            CreatedAtUtc = DateTime.UtcNow.AddDays(-5)
        };

        dbContext.AlertRules.Add(rule);
        return rule.Id;
    }

    private async Task SeedAlertIncidentAsync(
        Guid ruleId,
        Guid triggerEventId,
        DateTime now,
        CancellationToken cancellationToken)
    {
        bool exists = await dbContext.AlertIncidents
            .AnyAsync(x => x.RuleId == ruleId && x.TriggerEventId == triggerEventId, cancellationToken);

        if (exists)
        {
            return;
        }

        dbContext.AlertIncidents.Add(new AlertIncident
        {
            Id = Guid.Parse("D1885B4B-73C6-452D-B88E-2D9A4525EAB7"),
            RuleId = ruleId,
            TriggerEventId = triggerEventId,
            TriggeredAtUtc = now.AddMinutes(-15),
            Status = "Open",
            RetryCount = 1,
            NextRetryAtUtc = now.AddMinutes(5)
        });
    }

    private async Task<Guid> SeedNotificationsAsync(
        Guid adminUserId,
        Guid customerUserId,
        DateTime now,
        CancellationToken cancellationToken)
    {
        Guid? templateId = await dbContext.NotificationTemplates
            .Where(x => x.Name == NotificationTemplateCatalog.UserRegistrationVerificationEmail)
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        NotificationMessage? existing = await dbContext.NotificationMessages
            .SingleOrDefaultAsync(x => x.Subject == "[seed] Welcome to CleanArchitecture", cancellationToken);

        if (existing is not null)
        {
            return existing.Id;
        }

        NotificationMessage notification = new()
        {
            Id = Guid.Parse("962026DE-1257-421F-AE84-0A5F79B597D9"),
            CreatedByUserId = adminUserId,
            Channel = NotificationChannel.Email,
            Priority = NotificationPriority.Medium,
            Status = NotificationStatus.Scheduled,
            RecipientEncrypted = Convert.ToBase64String(Encoding.UTF8.GetBytes(CustomerEmail)),
            RecipientHash = ComputeSha256(CustomerEmail),
            Subject = "[seed] Welcome to CleanArchitecture",
            Body = "Your workspace has been provisioned successfully.",
            Language = "fa-IR",
            TemplateId = templateId,
            RetryCount = 0,
            MaxRetryCount = 5,
            CreatedAtUtc = now.AddMinutes(-25),
            ScheduledAtUtc = now.AddMinutes(8),
            LastError = null,
            IsArchived = false
        };

        dbContext.NotificationMessages.Add(notification);

        bool hasInApp = await dbContext.NotificationMessages
            .AnyAsync(x => x.Channel == NotificationChannel.InApp && x.CreatedByUserId == adminUserId, cancellationToken);
        if (!hasInApp)
        {
            dbContext.NotificationMessages.Add(new NotificationMessage
            {
                Id = Guid.Parse("12E3BB95-78E2-4DF2-9896-1568BCC32852"),
                CreatedByUserId = adminUserId,
                Channel = NotificationChannel.InApp,
                Priority = NotificationPriority.Low,
                Status = NotificationStatus.Delivered,
                RecipientEncrypted = Convert.ToBase64String(Encoding.UTF8.GetBytes(customerUserId.ToString("N"))),
                RecipientHash = ComputeSha256(customerUserId.ToString("N")),
                Subject = "[seed] Daily digest is ready",
                Body = "Check the dashboard for your daily activity digest.",
                Language = "fa-IR",
                TemplateId = null,
                RetryCount = 0,
                MaxRetryCount = 2,
                CreatedAtUtc = now.AddHours(-7),
                SentAtUtc = now.AddHours(-6).AddMinutes(55),
                DeliveredAtUtc = now.AddHours(-6).AddMinutes(54),
                IsArchived = false
            });
        }

        return notification.Id;
    }

    private async Task SeedNotificationPermissionsAsync(
        Guid notificationId,
        Guid customerUserId,
        CancellationToken cancellationToken)
    {
        HashSet<string> existing = [.. await dbContext.NotificationPermissionEntries
            .Select(x => $"{x.NotificationId}:{x.SubjectType}:{x.SubjectValue}")
            .ToListAsync(cancellationToken)];

        NotificationPermissionEntry[] entries =
        [
            new NotificationPermissionEntry
            {
                Id = Guid.Parse("D8D4F376-0337-4CA3-9A8A-400B5261F05F"),
                NotificationId = notificationId,
                SubjectType = "role",
                SubjectValue = "admin",
                CanRead = true,
                CanManage = true,
                CreatedAtUtc = DateTime.UtcNow.AddMinutes(-24)
            },
            new NotificationPermissionEntry
            {
                Id = Guid.Parse("5A7B96E4-EF4D-4A86-B8BB-F78F59C7CC05"),
                NotificationId = notificationId,
                SubjectType = "user",
                SubjectValue = customerUserId.ToString("N"),
                CanRead = true,
                CanManage = false,
                CreatedAtUtc = DateTime.UtcNow.AddMinutes(-24)
            }
        ];

        foreach (NotificationPermissionEntry entry in entries)
        {
            string key = $"{entry.NotificationId}:{entry.SubjectType}:{entry.SubjectValue}";
            if (existing.Add(key))
            {
                dbContext.NotificationPermissionEntries.Add(entry);
            }
        }
    }

    private async Task SeedNotificationScheduleAsync(
        Guid notificationId,
        DateTime now,
        CancellationToken cancellationToken)
    {
        bool exists = await dbContext.NotificationSchedules
            .AnyAsync(x => x.NotificationId == notificationId, cancellationToken);

        if (exists)
        {
            return;
        }

        dbContext.NotificationSchedules.Add(new NotificationSchedule
        {
            Id = Guid.Parse("C7B8E60A-5B9B-4CF9-9DB2-35A621C98B5C"),
            NotificationId = notificationId,
            RunAtUtc = now.AddMinutes(8),
            RuleName = "onboarding-welcome-sequence",
            IsCancelled = false,
            CreatedAtUtc = now.AddMinutes(-20)
        });
    }

    private async Task SeedNotificationDeliveryAttemptsAsync(
        Guid notificationId,
        DateTime now,
        CancellationToken cancellationToken)
    {
        bool exists = await dbContext.NotificationDeliveryAttempts
            .AnyAsync(x => x.NotificationId == notificationId, cancellationToken);

        if (exists)
        {
            return;
        }

        dbContext.NotificationDeliveryAttempts.AddRange(
            new NotificationDeliveryAttempt
            {
                Id = Guid.Parse("220A2C26-1A56-4E99-B6A8-5E88F7C81868"),
                NotificationId = notificationId,
                Channel = NotificationChannel.Email,
                AttemptNumber = 1,
                IsSuccess = false,
                Error = "SMTP timeout",
                CreatedAtUtc = now.AddMinutes(-12)
            },
            new NotificationDeliveryAttempt
            {
                Id = Guid.Parse("4BAA5E48-80A0-4DF0-B84A-E87E75F6B7AA"),
                NotificationId = notificationId,
                Channel = NotificationChannel.Email,
                AttemptNumber = 2,
                IsSuccess = true,
                ProviderMessageId = "seed-msg-20260221-0002",
                CreatedAtUtc = now.AddMinutes(-9)
            });
    }

    private async Task SeedSchedulerRuntimeAsync(
        Guid opsUserId,
        DateTime now,
        CancellationToken cancellationToken)
    {
        List<ScheduledJob> jobs = await dbContext.ScheduledJobs
            .Where(x => x.Name == "scheduler-cleanup-executions" || x.Name == "scheduler-notification-probe")
            .ToListAsync(cancellationToken);

        ScheduledJob? cleanupJob = jobs.SingleOrDefault(x => x.Name == "scheduler-cleanup-executions");
        ScheduledJob? probeJob = jobs.SingleOrDefault(x => x.Name == "scheduler-notification-probe");

        if (cleanupJob is not null)
        {
            await EnsureJobPermissionAsync(
                Guid.Parse("6764DACF-0442-4311-97A8-70E8B4D12E1E"),
                cleanupJob.Id,
                "role",
                "admin",
                canManage: true,
                cancellationToken);
        }

        if (probeJob is not null)
        {
            await EnsureJobPermissionAsync(
                Guid.Parse("2E334B8D-8CF3-4A69-92E6-6FAFE370D873"),
                probeJob.Id,
                "user",
                opsUserId.ToString("N"),
                canManage: false,
                cancellationToken);
        }

        if (cleanupJob is not null)
        {
            await EnsureJobExecutionAsync(
                Guid.Parse("B725A8DC-4C7F-4535-99A8-F4502487677C"),
                cleanupJob.Id,
                JobExecutionStatus.Succeeded,
                "seed-runner",
                now.AddHours(-5),
                now.AddHours(-5).AddSeconds(3),
                now.AddHours(-5).AddSeconds(4),
                durationMs: 1200,
                attempt: 1,
                maxAttempts: 3,
                error: null,
                cancellationToken);
        }

        if (probeJob is not null)
        {
            await EnsureJobExecutionAsync(
                Guid.Parse("11CC9B63-B5A0-4794-9D7A-BB327B4D6D3F"),
                probeJob.Id,
                JobExecutionStatus.Failed,
                "seed-runner",
                now.AddHours(-2),
                now.AddHours(-2).AddSeconds(2),
                now.AddHours(-2).AddSeconds(5),
                durationMs: 3100,
                attempt: 2,
                maxAttempts: 3,
                error: "Transient email provider outage",
                cancellationToken);
        }

        SchedulerLockLease? lockRow = await dbContext.SchedulerLockLeases
            .SingleOrDefaultAsync(x => x.LockName == "scheduler:dispatch-loop", cancellationToken);

        if (lockRow is null)
        {
            dbContext.SchedulerLockLeases.Add(new SchedulerLockLease
            {
                LockName = "scheduler:dispatch-loop",
                OwnerNodeId = "seed-node-a",
                AcquiredAtUtc = now.AddSeconds(-20),
                ExpiresAtUtc = now.AddSeconds(100)
            });
        }
        else
        {
            lockRow.OwnerNodeId = "seed-node-a";
            lockRow.AcquiredAtUtc = now.AddSeconds(-20);
            lockRow.ExpiresAtUtc = now.AddSeconds(100);
        }
    }

    private async Task EnsureJobPermissionAsync(
        Guid id,
        Guid jobId,
        string subjectType,
        string subjectValue,
        bool canManage,
        CancellationToken cancellationToken)
    {
        bool exists = await dbContext.JobPermissionEntries
            .AnyAsync(
                x => x.JobId == jobId &&
                     x.SubjectType == subjectType &&
                     x.SubjectValue == subjectValue,
                cancellationToken);

        if (exists)
        {
            return;
        }

        dbContext.JobPermissionEntries.Add(new JobPermissionEntry
        {
            Id = id,
            JobId = jobId,
            SubjectType = subjectType,
            SubjectValue = subjectValue,
            CanRead = true,
            CanManage = canManage,
            CreatedAtUtc = DateTime.UtcNow.AddDays(-2)
        });
    }

    private async Task EnsureJobExecutionAsync(
        Guid id,
        Guid jobId,
        JobExecutionStatus status,
        string triggeredBy,
        DateTime scheduledAtUtc,
        DateTime startedAtUtc,
        DateTime finishedAtUtc,
        int durationMs,
        int attempt,
        int maxAttempts,
        string? error,
        CancellationToken cancellationToken)
    {
        bool exists = await dbContext.JobExecutions.AnyAsync(x => x.Id == id, cancellationToken);
        if (exists)
        {
            return;
        }

        dbContext.JobExecutions.Add(new JobExecution
        {
            Id = id,
            JobId = jobId,
            Status = status,
            TriggeredBy = triggeredBy,
            NodeId = "seed-node-a",
            ScheduledAtUtc = scheduledAtUtc,
            StartedAtUtc = startedAtUtc,
            FinishedAtUtc = finishedAtUtc,
            DurationMs = durationMs,
            Attempt = attempt,
            MaxAttempts = maxAttempts,
            IsReplay = false,
            IsDeadLetter = status == JobExecutionStatus.DeadLettered,
            DeadLetterReason = status == JobExecutionStatus.DeadLettered ? "Manual seeding" : null,
            PayloadSnapshotJson = "{\"source\":\"seed\"}",
            Error = error
        });
    }

    private async Task SeedAuditTrailAsync(
        Guid adminUserId,
        Guid opsUserId,
        Guid notificationId,
        Guid logEventId,
        DateTime now,
        CancellationToken cancellationToken)
    {
        bool exists = await dbContext.AuditEntries
            .AnyAsync(x => x.ResourceType == "seed", cancellationToken);

        if (exists)
        {
            return;
        }

        string payload1 = ComputeSha256("{\"op\":\"seed-users\"}");
        string checksum1 = ComputeSha256($"{adminUserId:N}|SeedUsers|seed|users|{payload1}|GENESIS");
        string payload2 = ComputeSha256($"{{\"notificationId\":\"{notificationId:N}\"}}");
        string checksum2 = ComputeSha256($"{opsUserId:N}|SeedNotifications|seed|notifications|{payload2}|{checksum1}");
        string payload3 = ComputeSha256($"{{\"logEventId\":\"{logEventId:N}\"}}");
        string checksum3 = ComputeSha256($"{adminUserId:N}|SeedLogs|seed|logs|{payload3}|{checksum2}");

        dbContext.AuditEntries.AddRange(
            new AuditEntry
            {
                Id = Guid.Parse("54297A78-0064-4AB7-B53D-962B8455FAFB"),
                TimestampUtc = now.AddMinutes(-30),
                ActorId = adminUserId.ToString("N"),
                Action = "SeedUsers",
                ResourceType = "seed",
                ResourceId = "users",
                PayloadHash = payload1,
                PreviousChecksum = "GENESIS",
                Checksum = checksum1,
                IsTampered = false
            },
            new AuditEntry
            {
                Id = Guid.Parse("0E57C216-FA1C-4D88-ADCD-E3014223B3CF"),
                TimestampUtc = now.AddMinutes(-20),
                ActorId = opsUserId.ToString("N"),
                Action = "SeedNotifications",
                ResourceType = "seed",
                ResourceId = "notifications",
                PayloadHash = payload2,
                PreviousChecksum = checksum1,
                Checksum = checksum2,
                IsTampered = false
            },
            new AuditEntry
            {
                Id = Guid.Parse("CD32C1D0-9E6F-4FA8-B6D3-CC554A4D6FC8"),
                TimestampUtc = now.AddMinutes(-10),
                ActorId = adminUserId.ToString("N"),
                Action = "SeedLogs",
                ResourceType = "seed",
                ResourceId = "logs",
                PayloadHash = payload3,
                PreviousChecksum = checksum2,
                Checksum = checksum3,
                IsTampered = false
            });
    }

    private static UserProfile BuildProfile(
        Guid userId,
        string displayName,
        string location,
        string timeZone,
        string interests,
        bool isPublic,
        DateTime now)
    {
        return new UserProfile
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            DisplayName = displayName,
            Bio = $"Seeded profile for {displayName}.",
            DateOfBirth = new DateTime(1995, 5, 10, 0, 0, 0, DateTimeKind.Utc),
            Gender = "unspecified",
            Location = location,
            TimeZone = timeZone,
            PreferredLanguage = "fa-IR",
            WebsiteUrl = "example.local/profile",
            ContactEmail = $"{displayName.Replace(" ", ".")}@example.local",
            ContactPhone = "+989120000000",
            FavoriteMusicTitle = "Seed Anthem",
            FavoriteMusicArtist = "Sample Band",
            InterestsCsv = interests,
            SocialLinksJson = "{\"x\":\"https://x.com/seed\",\"github\":\"https://github.com/seed\"}",
            IsProfilePublic = isPublic,
            ShowEmail = false,
            ShowPhone = false,
            ReceiveSecurityAlerts = true,
            ReceiveProductUpdates = true,
            ProfileCompletenessScore = 86,
            LastSeenAtUtc = now.AddHours(-1),
            LastProfileUpdateAtUtc = now.AddDays(-1)
        };
    }

    private static LogEvent BuildLogEvent(
        Guid id,
        string idempotencyKey,
        DateTime timestampUtc,
        LogLevelType level,
        string message,
        string sourceService,
        string sourceModule,
        string actorId,
        string outcome)
    {
        string canonical = string.Join('|', timestampUtc.ToString("O"), level, message, sourceService, sourceModule, actorId, outcome);
        return new LogEvent
        {
            Id = id,
            IdempotencyKey = idempotencyKey,
            TimestampUtc = timestampUtc,
            Level = level,
            Message = message,
            SourceService = sourceService,
            SourceModule = sourceModule,
            TraceId = $"seed-trace-{idempotencyKey}",
            RequestId = $"seed-req-{idempotencyKey}",
            TenantId = "seed-tenant",
            ActorType = "user",
            ActorId = actorId,
            Outcome = outcome,
            SessionId = $"seed-session-{idempotencyKey}",
            Ip = "10.10.10.10",
            UserAgent = "SeedBot/1.0",
            DeviceInfo = "seed-runner",
            HttpMethod = "POST",
            HttpPath = "/api/v1/seed/simulated",
            HttpStatusCode = outcome == "failed" ? 500 : 200,
            ErrorCode = outcome == "failed" ? "SEED_NOTIFICATION_FAIL" : null,
            ErrorStackHash = outcome == "failed" ? ComputeSha256("seed-stack").Substring(0, 32) : null,
            TagsCsv = "seed,simulation",
            PayloadJson = "{\"seed\":true}",
            Checksum = ComputeSha256(canonical),
            HasIntegrityIssue = false,
            IsDeleted = false
        };
    }

    private static string ComputeSha256(string input)
    {
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(hash);
    }
}
