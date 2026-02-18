using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
#pragma warning disable IDE0161, CA1861

namespace Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NotificationTemplates",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Channel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Language = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SubjectTemplate = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    BodyTemplate = table.Column<string>(type: "nvarchar(max)", maxLength: 10000, nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NotificationMessages",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Channel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RecipientEncrypted = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    RecipientHash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Subject = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: false),
                    Language = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RetryCount = table.Column<int>(type: "int", nullable: false),
                    MaxRetryCount = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ScheduledAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SentAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeliveredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NextRetryAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastError = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    IsArchived = table.Column<bool>(type: "bit", nullable: false),
                    ArchivedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationMessages_NotificationTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalSchema: "dbo",
                        principalTable: "NotificationTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_NotificationMessages_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NotificationTemplateRevisions",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    SubjectTemplate = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    BodyTemplate = table.Column<string>(type: "nvarchar(max)", maxLength: 10000, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationTemplateRevisions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationTemplateRevisions_NotificationTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalSchema: "dbo",
                        principalTable: "NotificationTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NotificationTemplateRevisions_Users_ChangedByUserId",
                        column: x => x.ChangedByUserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "NotificationDeliveryAttempts",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NotificationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Channel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AttemptNumber = table.Column<int>(type: "int", nullable: false),
                    IsSuccess = table.Column<bool>(type: "bit", nullable: false),
                    ProviderMessageId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Error = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationDeliveryAttempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationDeliveryAttempts_NotificationMessages_NotificationId",
                        column: x => x.NotificationId,
                        principalSchema: "dbo",
                        principalTable: "NotificationMessages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NotificationPermissionEntries",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NotificationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubjectType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SubjectValue = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CanRead = table.Column<bool>(type: "bit", nullable: false),
                    CanManage = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationPermissionEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationPermissionEntries_NotificationMessages_NotificationId",
                        column: x => x.NotificationId,
                        principalSchema: "dbo",
                        principalTable: "NotificationMessages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NotificationSchedules",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NotificationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RunAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RuleName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsCancelled = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationSchedules_NotificationMessages_NotificationId",
                        column: x => x.NotificationId,
                        principalSchema: "dbo",
                        principalTable: "NotificationMessages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationDeliveryAttempts_IsSuccess_CreatedAtUtc",
                schema: "dbo",
                table: "NotificationDeliveryAttempts",
                columns: new[] { "IsSuccess", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationDeliveryAttempts_NotificationId_AttemptNumber",
                schema: "dbo",
                table: "NotificationDeliveryAttempts",
                columns: new[] { "NotificationId", "AttemptNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationMessages_Channel_Status",
                schema: "dbo",
                table: "NotificationMessages",
                columns: new[] { "Channel", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationMessages_CreatedByUserId",
                schema: "dbo",
                table: "NotificationMessages",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationMessages_Status_CreatedAtUtc",
                schema: "dbo",
                table: "NotificationMessages",
                columns: new[] { "Status", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationMessages_TemplateId",
                schema: "dbo",
                table: "NotificationMessages",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationPermissionEntries_NotificationId_SubjectType_SubjectValue",
                schema: "dbo",
                table: "NotificationPermissionEntries",
                columns: new[] { "NotificationId", "SubjectType", "SubjectValue" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NotificationSchedules_NotificationId",
                schema: "dbo",
                table: "NotificationSchedules",
                column: "NotificationId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationSchedules_RunAtUtc_IsCancelled",
                schema: "dbo",
                table: "NotificationSchedules",
                columns: new[] { "RunAtUtc", "IsCancelled" });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTemplateRevisions_ChangedByUserId",
                schema: "dbo",
                table: "NotificationTemplateRevisions",
                column: "ChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTemplateRevisions_TemplateId_Version",
                schema: "dbo",
                table: "NotificationTemplateRevisions",
                columns: new[] { "TemplateId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTemplates_IsDeleted",
                schema: "dbo",
                table: "NotificationTemplates",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTemplates_Name_Language_Channel",
                schema: "dbo",
                table: "NotificationTemplates",
                columns: new[] { "Name", "Language", "Channel" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NotificationDeliveryAttempts",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "NotificationPermissionEntries",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "NotificationSchedules",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "NotificationTemplateRevisions",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "NotificationMessages",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "NotificationTemplates",
                schema: "dbo");
        }
    }
}
#pragma warning restore IDE0161, CA1861
