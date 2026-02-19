using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Database.Migrations;
    /// <inheritdoc />
    public partial class ModuleBasedSchemas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "logging");

            migrationBuilder.EnsureSchema(
                name: "audit");

            migrationBuilder.EnsureSchema(
                name: "files");

            migrationBuilder.EnsureSchema(
                name: "integration");

            migrationBuilder.EnsureSchema(
                name: "notifications");

            migrationBuilder.EnsureSchema(
                name: "auth");

            migrationBuilder.EnsureSchema(
                name: "todos");

            migrationBuilder.EnsureSchema(
                name: "users");

            migrationBuilder.RenameTable(
                name: "Users",
                schema: "dbo",
                newName: "Users",
                newSchema: "users");

            migrationBuilder.RenameTable(
                name: "UserRoles",
                schema: "dbo",
                newName: "UserRoles",
                newSchema: "auth");

            migrationBuilder.RenameTable(
                name: "UserPermissions",
                schema: "dbo",
                newName: "UserPermissions",
                newSchema: "auth");

            migrationBuilder.RenameTable(
                name: "UserExternalLogins",
                schema: "dbo",
                newName: "UserExternalLogins",
                newSchema: "users");

            migrationBuilder.RenameTable(
                name: "TodoItems",
                schema: "dbo",
                newName: "TodoItems",
                newSchema: "todos");

            migrationBuilder.RenameTable(
                name: "Roles",
                schema: "dbo",
                newName: "Roles",
                newSchema: "auth");

            migrationBuilder.RenameTable(
                name: "RolePermissions",
                schema: "dbo",
                newName: "RolePermissions",
                newSchema: "auth");

            migrationBuilder.RenameTable(
                name: "RefreshTokens",
                schema: "dbo",
                newName: "RefreshTokens",
                newSchema: "auth");

            migrationBuilder.RenameTable(
                name: "Permissions",
                schema: "dbo",
                newName: "Permissions",
                newSchema: "auth");

            migrationBuilder.RenameTable(
                name: "OutboxMessages",
                schema: "dbo",
                newName: "OutboxMessages",
                newSchema: "integration");

            migrationBuilder.RenameTable(
                name: "NotificationTemplates",
                schema: "dbo",
                newName: "NotificationTemplates",
                newSchema: "notifications");

            migrationBuilder.RenameTable(
                name: "NotificationTemplateRevisions",
                schema: "dbo",
                newName: "NotificationTemplateRevisions",
                newSchema: "notifications");

            migrationBuilder.RenameTable(
                name: "NotificationSchedules",
                schema: "dbo",
                newName: "NotificationSchedules",
                newSchema: "notifications");

            migrationBuilder.RenameTable(
                name: "NotificationPermissionEntries",
                schema: "dbo",
                newName: "NotificationPermissionEntries",
                newSchema: "notifications");

            migrationBuilder.RenameTable(
                name: "NotificationMessages",
                schema: "dbo",
                newName: "NotificationMessages",
                newSchema: "notifications");

            migrationBuilder.RenameTable(
                name: "NotificationDeliveryAttempts",
                schema: "dbo",
                newName: "NotificationDeliveryAttempts",
                newSchema: "notifications");

            migrationBuilder.RenameTable(
                name: "LogEvents",
                schema: "dbo",
                newName: "LogEvents",
                newSchema: "logging");

            migrationBuilder.RenameTable(
                name: "InboxMessages",
                schema: "dbo",
                newName: "InboxMessages",
                newSchema: "integration");

            migrationBuilder.RenameTable(
                name: "FileTags",
                schema: "dbo",
                newName: "FileTags",
                newSchema: "files");

            migrationBuilder.RenameTable(
                name: "FilePermissionEntries",
                schema: "dbo",
                newName: "FilePermissionEntries",
                newSchema: "files");

            migrationBuilder.RenameTable(
                name: "FileAssets",
                schema: "dbo",
                newName: "FileAssets",
                newSchema: "files");

            migrationBuilder.RenameTable(
                name: "FileAccessAudits",
                schema: "dbo",
                newName: "FileAccessAudits",
                newSchema: "files");

            migrationBuilder.RenameTable(
                name: "AuditEntries",
                schema: "dbo",
                newName: "AuditEntries",
                newSchema: "audit");

            migrationBuilder.RenameTable(
                name: "AlertRules",
                schema: "dbo",
                newName: "AlertRules",
                newSchema: "logging");

            migrationBuilder.RenameTable(
                name: "AlertIncidents",
                schema: "dbo",
                newName: "AlertIncidents",
                newSchema: "logging");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.RenameTable(
                name: "Users",
                schema: "users",
                newName: "Users",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "UserRoles",
                schema: "auth",
                newName: "UserRoles",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "UserPermissions",
                schema: "auth",
                newName: "UserPermissions",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "UserExternalLogins",
                schema: "users",
                newName: "UserExternalLogins",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "TodoItems",
                schema: "todos",
                newName: "TodoItems",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "Roles",
                schema: "auth",
                newName: "Roles",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "RolePermissions",
                schema: "auth",
                newName: "RolePermissions",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "RefreshTokens",
                schema: "auth",
                newName: "RefreshTokens",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "Permissions",
                schema: "auth",
                newName: "Permissions",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "OutboxMessages",
                schema: "integration",
                newName: "OutboxMessages",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "NotificationTemplates",
                schema: "notifications",
                newName: "NotificationTemplates",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "NotificationTemplateRevisions",
                schema: "notifications",
                newName: "NotificationTemplateRevisions",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "NotificationSchedules",
                schema: "notifications",
                newName: "NotificationSchedules",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "NotificationPermissionEntries",
                schema: "notifications",
                newName: "NotificationPermissionEntries",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "NotificationMessages",
                schema: "notifications",
                newName: "NotificationMessages",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "NotificationDeliveryAttempts",
                schema: "notifications",
                newName: "NotificationDeliveryAttempts",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "LogEvents",
                schema: "logging",
                newName: "LogEvents",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "InboxMessages",
                schema: "integration",
                newName: "InboxMessages",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "FileTags",
                schema: "files",
                newName: "FileTags",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "FilePermissionEntries",
                schema: "files",
                newName: "FilePermissionEntries",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "FileAssets",
                schema: "files",
                newName: "FileAssets",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "FileAccessAudits",
                schema: "files",
                newName: "FileAccessAudits",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "AuditEntries",
                schema: "audit",
                newName: "AuditEntries",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "AlertRules",
                schema: "logging",
                newName: "AlertRules",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "AlertIncidents",
                schema: "logging",
                newName: "AlertIncidents",
                newSchema: "dbo");
        }
    }
