using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class AddIdempotencyRequests : Migration
{
    private static readonly string[] IdempotencyStatusColumns = ["IsCompleted", "CreatedAtUtc"];
    private static readonly string[] IdempotencyScopeColumns = ["ScopeHash", "Key"];

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "IdempotencyRequests",
            schema: "integration",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Key = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                ScopeHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                Scope = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                ExpiresAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                CompletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                StatusCode = table.Column<int>(type: "int", nullable: true),
                ContentType = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                ResponseBody = table.Column<byte[]>(type: "varbinary(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_IdempotencyRequests", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_IdempotencyRequests_ExpiresAtUtc",
            schema: "integration",
            table: "IdempotencyRequests",
            column: "ExpiresAtUtc");

        migrationBuilder.CreateIndex(
            name: "IX_IdempotencyRequests_IsCompleted_CreatedAtUtc",
            schema: "integration",
            table: "IdempotencyRequests",
            columns: IdempotencyStatusColumns);

        migrationBuilder.CreateIndex(
            name: "IX_IdempotencyRequests_ScopeHash_Key",
            schema: "integration",
            table: "IdempotencyRequests",
            columns: IdempotencyScopeColumns,
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "IdempotencyRequests",
            schema: "integration");
    }
}
