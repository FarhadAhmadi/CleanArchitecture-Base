using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable IDE0161

namespace Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddEntityAuditFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AuditCreatedAtUtc",
                schema: "users",
                table: "Users",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "AuditCreatedBy",
                schema: "users",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AuditUpdatedAtUtc",
                schema: "users",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuditUpdatedBy",
                schema: "users",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AuditCreatedAtUtc",
                schema: "users",
                table: "UserExternalLogins",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "AuditCreatedBy",
                schema: "users",
                table: "UserExternalLogins",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AuditUpdatedAtUtc",
                schema: "users",
                table: "UserExternalLogins",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuditUpdatedBy",
                schema: "users",
                table: "UserExternalLogins",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AuditCreatedAtUtc",
                schema: "todos",
                table: "TodoItems",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "AuditCreatedBy",
                schema: "todos",
                table: "TodoItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AuditUpdatedAtUtc",
                schema: "todos",
                table: "TodoItems",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuditUpdatedBy",
                schema: "todos",
                table: "TodoItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AuditCreatedAtUtc",
                schema: "auth",
                table: "Roles",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "AuditCreatedBy",
                schema: "auth",
                table: "Roles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AuditUpdatedAtUtc",
                schema: "auth",
                table: "Roles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuditUpdatedBy",
                schema: "auth",
                table: "Roles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AuditCreatedAtUtc",
                schema: "auth",
                table: "RefreshTokens",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "AuditCreatedBy",
                schema: "auth",
                table: "RefreshTokens",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AuditUpdatedAtUtc",
                schema: "auth",
                table: "RefreshTokens",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuditUpdatedBy",
                schema: "auth",
                table: "RefreshTokens",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AuditCreatedAtUtc",
                schema: "auth",
                table: "Permissions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "AuditCreatedBy",
                schema: "auth",
                table: "Permissions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AuditUpdatedAtUtc",
                schema: "auth",
                table: "Permissions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuditUpdatedBy",
                schema: "auth",
                table: "Permissions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AuditCreatedAtUtc",
                schema: "notifications",
                table: "NotificationTemplates",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "AuditCreatedBy",
                schema: "notifications",
                table: "NotificationTemplates",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AuditUpdatedAtUtc",
                schema: "notifications",
                table: "NotificationTemplates",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuditUpdatedBy",
                schema: "notifications",
                table: "NotificationTemplates",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AuditCreatedAtUtc",
                schema: "notifications",
                table: "NotificationTemplateRevisions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "AuditCreatedBy",
                schema: "notifications",
                table: "NotificationTemplateRevisions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AuditUpdatedAtUtc",
                schema: "notifications",
                table: "NotificationTemplateRevisions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuditUpdatedBy",
                schema: "notifications",
                table: "NotificationTemplateRevisions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AuditCreatedAtUtc",
                schema: "notifications",
                table: "NotificationSchedules",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "AuditCreatedBy",
                schema: "notifications",
                table: "NotificationSchedules",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AuditUpdatedAtUtc",
                schema: "notifications",
                table: "NotificationSchedules",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuditUpdatedBy",
                schema: "notifications",
                table: "NotificationSchedules",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AuditCreatedAtUtc",
                schema: "notifications",
                table: "NotificationPermissionEntries",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "AuditCreatedBy",
                schema: "notifications",
                table: "NotificationPermissionEntries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AuditUpdatedAtUtc",
                schema: "notifications",
                table: "NotificationPermissionEntries",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuditUpdatedBy",
                schema: "notifications",
                table: "NotificationPermissionEntries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AuditCreatedAtUtc",
                schema: "notifications",
                table: "NotificationMessages",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "AuditCreatedBy",
                schema: "notifications",
                table: "NotificationMessages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AuditUpdatedAtUtc",
                schema: "notifications",
                table: "NotificationMessages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuditUpdatedBy",
                schema: "notifications",
                table: "NotificationMessages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AuditCreatedAtUtc",
                schema: "notifications",
                table: "NotificationDeliveryAttempts",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "AuditCreatedBy",
                schema: "notifications",
                table: "NotificationDeliveryAttempts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AuditUpdatedAtUtc",
                schema: "notifications",
                table: "NotificationDeliveryAttempts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuditUpdatedBy",
                schema: "notifications",
                table: "NotificationDeliveryAttempts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AuditCreatedAtUtc",
                schema: "logging",
                table: "LogEvents",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "AuditCreatedBy",
                schema: "logging",
                table: "LogEvents",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AuditUpdatedAtUtc",
                schema: "logging",
                table: "LogEvents",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuditUpdatedBy",
                schema: "logging",
                table: "LogEvents",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AuditCreatedAtUtc",
                schema: "files",
                table: "FilePermissionEntries",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "AuditCreatedBy",
                schema: "files",
                table: "FilePermissionEntries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AuditUpdatedAtUtc",
                schema: "files",
                table: "FilePermissionEntries",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuditUpdatedBy",
                schema: "files",
                table: "FilePermissionEntries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AuditCreatedAtUtc",
                schema: "files",
                table: "FileAssets",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "AuditCreatedBy",
                schema: "files",
                table: "FileAssets",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AuditUpdatedAtUtc",
                schema: "files",
                table: "FileAssets",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuditUpdatedBy",
                schema: "files",
                table: "FileAssets",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AuditCreatedAtUtc",
                schema: "files",
                table: "FileAccessAudits",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "AuditCreatedBy",
                schema: "files",
                table: "FileAccessAudits",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AuditUpdatedAtUtc",
                schema: "files",
                table: "FileAccessAudits",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuditUpdatedBy",
                schema: "files",
                table: "FileAccessAudits",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AuditCreatedAtUtc",
                schema: "audit",
                table: "AuditEntries",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "AuditCreatedBy",
                schema: "audit",
                table: "AuditEntries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AuditUpdatedAtUtc",
                schema: "audit",
                table: "AuditEntries",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuditUpdatedBy",
                schema: "audit",
                table: "AuditEntries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AuditCreatedAtUtc",
                schema: "logging",
                table: "AlertRules",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "AuditCreatedBy",
                schema: "logging",
                table: "AlertRules",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AuditUpdatedAtUtc",
                schema: "logging",
                table: "AlertRules",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuditUpdatedBy",
                schema: "logging",
                table: "AlertRules",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AuditCreatedAtUtc",
                schema: "logging",
                table: "AlertIncidents",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "AuditCreatedBy",
                schema: "logging",
                table: "AlertIncidents",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AuditUpdatedAtUtc",
                schema: "logging",
                table: "AlertIncidents",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuditUpdatedBy",
                schema: "logging",
                table: "AlertIncidents",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuditCreatedAtUtc",
                schema: "users",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AuditCreatedBy",
                schema: "users",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AuditUpdatedAtUtc",
                schema: "users",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AuditUpdatedBy",
                schema: "users",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AuditCreatedAtUtc",
                schema: "users",
                table: "UserExternalLogins");

            migrationBuilder.DropColumn(
                name: "AuditCreatedBy",
                schema: "users",
                table: "UserExternalLogins");

            migrationBuilder.DropColumn(
                name: "AuditUpdatedAtUtc",
                schema: "users",
                table: "UserExternalLogins");

            migrationBuilder.DropColumn(
                name: "AuditUpdatedBy",
                schema: "users",
                table: "UserExternalLogins");

            migrationBuilder.DropColumn(
                name: "AuditCreatedAtUtc",
                schema: "todos",
                table: "TodoItems");

            migrationBuilder.DropColumn(
                name: "AuditCreatedBy",
                schema: "todos",
                table: "TodoItems");

            migrationBuilder.DropColumn(
                name: "AuditUpdatedAtUtc",
                schema: "todos",
                table: "TodoItems");

            migrationBuilder.DropColumn(
                name: "AuditUpdatedBy",
                schema: "todos",
                table: "TodoItems");

            migrationBuilder.DropColumn(
                name: "AuditCreatedAtUtc",
                schema: "auth",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "AuditCreatedBy",
                schema: "auth",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "AuditUpdatedAtUtc",
                schema: "auth",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "AuditUpdatedBy",
                schema: "auth",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "AuditCreatedAtUtc",
                schema: "auth",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "AuditCreatedBy",
                schema: "auth",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "AuditUpdatedAtUtc",
                schema: "auth",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "AuditUpdatedBy",
                schema: "auth",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "AuditCreatedAtUtc",
                schema: "auth",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "AuditCreatedBy",
                schema: "auth",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "AuditUpdatedAtUtc",
                schema: "auth",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "AuditUpdatedBy",
                schema: "auth",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "AuditCreatedAtUtc",
                schema: "notifications",
                table: "NotificationTemplates");

            migrationBuilder.DropColumn(
                name: "AuditCreatedBy",
                schema: "notifications",
                table: "NotificationTemplates");

            migrationBuilder.DropColumn(
                name: "AuditUpdatedAtUtc",
                schema: "notifications",
                table: "NotificationTemplates");

            migrationBuilder.DropColumn(
                name: "AuditUpdatedBy",
                schema: "notifications",
                table: "NotificationTemplates");

            migrationBuilder.DropColumn(
                name: "AuditCreatedAtUtc",
                schema: "notifications",
                table: "NotificationTemplateRevisions");

            migrationBuilder.DropColumn(
                name: "AuditCreatedBy",
                schema: "notifications",
                table: "NotificationTemplateRevisions");

            migrationBuilder.DropColumn(
                name: "AuditUpdatedAtUtc",
                schema: "notifications",
                table: "NotificationTemplateRevisions");

            migrationBuilder.DropColumn(
                name: "AuditUpdatedBy",
                schema: "notifications",
                table: "NotificationTemplateRevisions");

            migrationBuilder.DropColumn(
                name: "AuditCreatedAtUtc",
                schema: "notifications",
                table: "NotificationSchedules");

            migrationBuilder.DropColumn(
                name: "AuditCreatedBy",
                schema: "notifications",
                table: "NotificationSchedules");

            migrationBuilder.DropColumn(
                name: "AuditUpdatedAtUtc",
                schema: "notifications",
                table: "NotificationSchedules");

            migrationBuilder.DropColumn(
                name: "AuditUpdatedBy",
                schema: "notifications",
                table: "NotificationSchedules");

            migrationBuilder.DropColumn(
                name: "AuditCreatedAtUtc",
                schema: "notifications",
                table: "NotificationPermissionEntries");

            migrationBuilder.DropColumn(
                name: "AuditCreatedBy",
                schema: "notifications",
                table: "NotificationPermissionEntries");

            migrationBuilder.DropColumn(
                name: "AuditUpdatedAtUtc",
                schema: "notifications",
                table: "NotificationPermissionEntries");

            migrationBuilder.DropColumn(
                name: "AuditUpdatedBy",
                schema: "notifications",
                table: "NotificationPermissionEntries");

            migrationBuilder.DropColumn(
                name: "AuditCreatedAtUtc",
                schema: "notifications",
                table: "NotificationMessages");

            migrationBuilder.DropColumn(
                name: "AuditCreatedBy",
                schema: "notifications",
                table: "NotificationMessages");

            migrationBuilder.DropColumn(
                name: "AuditUpdatedAtUtc",
                schema: "notifications",
                table: "NotificationMessages");

            migrationBuilder.DropColumn(
                name: "AuditUpdatedBy",
                schema: "notifications",
                table: "NotificationMessages");

            migrationBuilder.DropColumn(
                name: "AuditCreatedAtUtc",
                schema: "notifications",
                table: "NotificationDeliveryAttempts");

            migrationBuilder.DropColumn(
                name: "AuditCreatedBy",
                schema: "notifications",
                table: "NotificationDeliveryAttempts");

            migrationBuilder.DropColumn(
                name: "AuditUpdatedAtUtc",
                schema: "notifications",
                table: "NotificationDeliveryAttempts");

            migrationBuilder.DropColumn(
                name: "AuditUpdatedBy",
                schema: "notifications",
                table: "NotificationDeliveryAttempts");

            migrationBuilder.DropColumn(
                name: "AuditCreatedAtUtc",
                schema: "logging",
                table: "LogEvents");

            migrationBuilder.DropColumn(
                name: "AuditCreatedBy",
                schema: "logging",
                table: "LogEvents");

            migrationBuilder.DropColumn(
                name: "AuditUpdatedAtUtc",
                schema: "logging",
                table: "LogEvents");

            migrationBuilder.DropColumn(
                name: "AuditUpdatedBy",
                schema: "logging",
                table: "LogEvents");

            migrationBuilder.DropColumn(
                name: "AuditCreatedAtUtc",
                schema: "files",
                table: "FilePermissionEntries");

            migrationBuilder.DropColumn(
                name: "AuditCreatedBy",
                schema: "files",
                table: "FilePermissionEntries");

            migrationBuilder.DropColumn(
                name: "AuditUpdatedAtUtc",
                schema: "files",
                table: "FilePermissionEntries");

            migrationBuilder.DropColumn(
                name: "AuditUpdatedBy",
                schema: "files",
                table: "FilePermissionEntries");

            migrationBuilder.DropColumn(
                name: "AuditCreatedAtUtc",
                schema: "files",
                table: "FileAssets");

            migrationBuilder.DropColumn(
                name: "AuditCreatedBy",
                schema: "files",
                table: "FileAssets");

            migrationBuilder.DropColumn(
                name: "AuditUpdatedAtUtc",
                schema: "files",
                table: "FileAssets");

            migrationBuilder.DropColumn(
                name: "AuditUpdatedBy",
                schema: "files",
                table: "FileAssets");

            migrationBuilder.DropColumn(
                name: "AuditCreatedAtUtc",
                schema: "files",
                table: "FileAccessAudits");

            migrationBuilder.DropColumn(
                name: "AuditCreatedBy",
                schema: "files",
                table: "FileAccessAudits");

            migrationBuilder.DropColumn(
                name: "AuditUpdatedAtUtc",
                schema: "files",
                table: "FileAccessAudits");

            migrationBuilder.DropColumn(
                name: "AuditUpdatedBy",
                schema: "files",
                table: "FileAccessAudits");

            migrationBuilder.DropColumn(
                name: "AuditCreatedAtUtc",
                schema: "audit",
                table: "AuditEntries");

            migrationBuilder.DropColumn(
                name: "AuditCreatedBy",
                schema: "audit",
                table: "AuditEntries");

            migrationBuilder.DropColumn(
                name: "AuditUpdatedAtUtc",
                schema: "audit",
                table: "AuditEntries");

            migrationBuilder.DropColumn(
                name: "AuditUpdatedBy",
                schema: "audit",
                table: "AuditEntries");

            migrationBuilder.DropColumn(
                name: "AuditCreatedAtUtc",
                schema: "logging",
                table: "AlertRules");

            migrationBuilder.DropColumn(
                name: "AuditCreatedBy",
                schema: "logging",
                table: "AlertRules");

            migrationBuilder.DropColumn(
                name: "AuditUpdatedAtUtc",
                schema: "logging",
                table: "AlertRules");

            migrationBuilder.DropColumn(
                name: "AuditUpdatedBy",
                schema: "logging",
                table: "AlertRules");

            migrationBuilder.DropColumn(
                name: "AuditCreatedAtUtc",
                schema: "logging",
                table: "AlertIncidents");

            migrationBuilder.DropColumn(
                name: "AuditCreatedBy",
                schema: "logging",
                table: "AlertIncidents");

            migrationBuilder.DropColumn(
                name: "AuditUpdatedAtUtc",
                schema: "logging",
                table: "AlertIncidents");

            migrationBuilder.DropColumn(
                name: "AuditUpdatedBy",
                schema: "logging",
                table: "AlertIncidents");
        }
    }
}

#pragma warning restore IDE0161
