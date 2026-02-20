using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Database.Migrations;

public partial class AddProfileModule : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(name: "profiles");

        migrationBuilder.CreateTable(
            name: "UserProfiles",
            schema: "profiles",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                DisplayName = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                Bio = table.Column<string>(type: "nvarchar(1200)", maxLength: 1200, nullable: true),
                AvatarUrl = table.Column<string>(type: "nvarchar(800)", maxLength: 800, nullable: true),
                DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                Gender = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                Location = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                TimeZone = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                PreferredLanguage = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                WebsiteUrl = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                ContactEmail = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: true),
                ContactPhone = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                InterestsCsv = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: false),
                SocialLinksJson = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                IsProfilePublic = table.Column<bool>(type: "bit", nullable: false),
                ShowEmail = table.Column<bool>(type: "bit", nullable: false),
                ShowPhone = table.Column<bool>(type: "bit", nullable: false),
                ReceiveSecurityAlerts = table.Column<bool>(type: "bit", nullable: false),
                ReceiveProductUpdates = table.Column<bool>(type: "bit", nullable: false),
                ProfileCompletenessScore = table.Column<int>(type: "int", nullable: false),
                LastSeenAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                LastProfileUpdateAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                AuditCreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                AuditCreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                AuditUpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                AuditUpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UserProfiles", x => x.Id);
                table.ForeignKey(
                    name: "FK_UserProfiles_Users_UserId",
                    column: x => x.UserId,
                    principalSchema: "users",
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_UserProfiles_DisplayName",
            schema: "profiles",
            table: "UserProfiles",
            column: "DisplayName");

        migrationBuilder.CreateIndex(
            name: "IX_UserProfiles_IsProfilePublic",
            schema: "profiles",
            table: "UserProfiles",
            column: "IsProfilePublic");

        migrationBuilder.CreateIndex(
            name: "IX_UserProfiles_LastProfileUpdateAtUtc",
            schema: "profiles",
            table: "UserProfiles",
            column: "LastProfileUpdateAtUtc");

        migrationBuilder.CreateIndex(
            name: "IX_UserProfiles_UserId",
            schema: "profiles",
            table: "UserProfiles",
            column: "UserId",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "UserProfiles",
            schema: "profiles");
    }
}
