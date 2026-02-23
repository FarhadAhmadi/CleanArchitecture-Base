using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Database.Migrations;

public partial class SyncPendingModelChanges : Migration
{
    private static readonly string[] UserPasswordHistoriesUserIdCreatedAtUtcColumns =
    [
        "UserId",
        "CreatedAtUtc"
    ];

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "UserPasswordHistories",
            schema: "auth",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                PasswordHash = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                AuditCreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                AuditCreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                AuditUpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                AuditUpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UserPasswordHistories", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_UserPasswordHistories_UserId_CreatedAtUtc",
            schema: "auth",
            table: "UserPasswordHistories",
            columns: UserPasswordHistoriesUserIdCreatedAtUtcColumns);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "UserPasswordHistories",
            schema: "auth");
    }
}
