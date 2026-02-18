using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
#pragma warning disable IDE0161, CA1861

namespace Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AdjustEntitySchemasAndReadWriteDal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserExternalLogins_UserId",
                schema: "dbo",
                table: "UserExternalLogins");

            migrationBuilder.DropIndex(
                name: "IX_TodoItems_UserId",
                schema: "dbo",
                table: "TodoItems");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                schema: "dbo",
                table: "TodoItems",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_UserExternalLogins_UserId_Provider",
                schema: "dbo",
                table: "UserExternalLogins",
                columns: new[] { "UserId", "Provider" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TodoItems_CreatedAt",
                schema: "dbo",
                table: "TodoItems",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_TodoItems_UserId_IsCompleted",
                schema: "dbo",
                table: "TodoItems",
                columns: new[] { "UserId", "IsCompleted" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditEntries_Checksum",
                schema: "dbo",
                table: "AuditEntries",
                column: "Checksum",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AlertIncidents_TriggerEventId",
                schema: "dbo",
                table: "AlertIncidents",
                column: "TriggerEventId");

            migrationBuilder.AddForeignKey(
                name: "FK_AlertIncidents_AlertRules_RuleId",
                schema: "dbo",
                table: "AlertIncidents",
                column: "RuleId",
                principalSchema: "dbo",
                principalTable: "AlertRules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AlertIncidents_LogEvents_TriggerEventId",
                schema: "dbo",
                table: "AlertIncidents",
                column: "TriggerEventId",
                principalSchema: "dbo",
                principalTable: "LogEvents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserExternalLogins_Users_UserId",
                schema: "dbo",
                table: "UserExternalLogins",
                column: "UserId",
                principalSchema: "dbo",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AlertIncidents_AlertRules_RuleId",
                schema: "dbo",
                table: "AlertIncidents");

            migrationBuilder.DropForeignKey(
                name: "FK_AlertIncidents_LogEvents_TriggerEventId",
                schema: "dbo",
                table: "AlertIncidents");

            migrationBuilder.DropForeignKey(
                name: "FK_UserExternalLogins_Users_UserId",
                schema: "dbo",
                table: "UserExternalLogins");

            migrationBuilder.DropIndex(
                name: "IX_UserExternalLogins_UserId_Provider",
                schema: "dbo",
                table: "UserExternalLogins");

            migrationBuilder.DropIndex(
                name: "IX_TodoItems_CreatedAt",
                schema: "dbo",
                table: "TodoItems");

            migrationBuilder.DropIndex(
                name: "IX_TodoItems_UserId_IsCompleted",
                schema: "dbo",
                table: "TodoItems");

            migrationBuilder.DropIndex(
                name: "IX_AuditEntries_Checksum",
                schema: "dbo",
                table: "AuditEntries");

            migrationBuilder.DropIndex(
                name: "IX_AlertIncidents_TriggerEventId",
                schema: "dbo",
                table: "AlertIncidents");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                schema: "dbo",
                table: "TodoItems",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000);

            migrationBuilder.CreateIndex(
                name: "IX_UserExternalLogins_UserId",
                schema: "dbo",
                table: "UserExternalLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TodoItems_UserId",
                schema: "dbo",
                table: "TodoItems",
                column: "UserId");
        }
    }
}
#pragma warning restore IDE0161, CA1861
