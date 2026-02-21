using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
#pragma warning disable CA1861

namespace Infrastructure.Database.Migrations;

    /// <inheritdoc />
    public partial class AddSchedulerRuntimeGovernance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "scheduler");

            migrationBuilder.CreateTable(
                name: "ScheduledJobs",
                schema: "scheduler",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PayloadJson = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastRunAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastExecutionStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    MaxRetryAttempts = table.Column<int>(type: "int", nullable: false),
                    RetryBackoffSeconds = table.Column<int>(type: "int", nullable: false),
                    MaxExecutionSeconds = table.Column<int>(type: "int", nullable: false),
                    MaxConsecutiveFailures = table.Column<int>(type: "int", nullable: false),
                    ConsecutiveFailures = table.Column<int>(type: "int", nullable: false),
                    IsQuarantined = table.Column<bool>(type: "bit", nullable: false),
                    QuarantinedUntilUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastFailureAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeadLetterReason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    AuditCreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AuditCreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AuditUpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AuditUpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduledJobs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SchedulerLocks",
                schema: "scheduler",
                columns: table => new
                {
                    LockName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    OwnerNodeId = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    AcquiredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchedulerLocks", x => x.LockName);
                });

            migrationBuilder.CreateTable(
                name: "JobDependencies",
                schema: "scheduler",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JobId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DependsOnJobId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AuditCreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AuditCreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AuditUpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AuditUpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobDependencies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobDependencies_ScheduledJobs_DependsOnJobId",
                        column: x => x.DependsOnJobId,
                        principalSchema: "scheduler",
                        principalTable: "ScheduledJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JobDependencies_ScheduledJobs_JobId",
                        column: x => x.JobId,
                        principalSchema: "scheduler",
                        principalTable: "ScheduledJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JobExecutions",
                schema: "scheduler",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JobId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TriggeredBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NodeId = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    ScheduledAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FinishedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DurationMs = table.Column<int>(type: "int", nullable: false),
                    Attempt = table.Column<int>(type: "int", nullable: false),
                    MaxAttempts = table.Column<int>(type: "int", nullable: false),
                    IsReplay = table.Column<bool>(type: "bit", nullable: false),
                    IsDeadLetter = table.Column<bool>(type: "bit", nullable: false),
                    DeadLetterReason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    PayloadSnapshotJson = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: true),
                    Error = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    AuditCreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AuditCreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AuditUpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AuditUpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobExecutions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobExecutions_ScheduledJobs_JobId",
                        column: x => x.JobId,
                        principalSchema: "scheduler",
                        principalTable: "ScheduledJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JobPermissionEntries",
                schema: "scheduler",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JobId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubjectType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SubjectValue = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CanRead = table.Column<bool>(type: "bit", nullable: false),
                    CanManage = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AuditCreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AuditCreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AuditUpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AuditUpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobPermissionEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobPermissionEntries_ScheduledJobs_JobId",
                        column: x => x.JobId,
                        principalSchema: "scheduler",
                        principalTable: "ScheduledJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JobSchedules",
                schema: "scheduler",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JobId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CronExpression = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    IntervalSeconds = table.Column<int>(type: "int", nullable: true),
                    OneTimeAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StartAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NextRunAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    MisfirePolicy = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MaxCatchUpRuns = table.Column<int>(type: "int", nullable: false),
                    RetryAttempt = table.Column<int>(type: "int", nullable: false),
                    LastMisfireAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AuditCreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AuditCreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AuditUpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AuditUpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobSchedules_ScheduledJobs_JobId",
                        column: x => x.JobId,
                        principalSchema: "scheduler",
                        principalTable: "ScheduledJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobDependencies_DependsOnJobId",
                schema: "scheduler",
                table: "JobDependencies",
                column: "DependsOnJobId");

            migrationBuilder.CreateIndex(
                name: "IX_JobDependencies_JobId_DependsOnJobId",
                schema: "scheduler",
                table: "JobDependencies",
                columns: new[] { "JobId", "DependsOnJobId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JobExecutions_IsDeadLetter",
                schema: "scheduler",
                table: "JobExecutions",
                column: "IsDeadLetter");

            migrationBuilder.CreateIndex(
                name: "IX_JobExecutions_JobId_ScheduledAtUtc",
                schema: "scheduler",
                table: "JobExecutions",
                columns: new[] { "JobId", "ScheduledAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_JobExecutions_JobId_StartedAtUtc",
                schema: "scheduler",
                table: "JobExecutions",
                columns: new[] { "JobId", "StartedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_JobExecutions_Status",
                schema: "scheduler",
                table: "JobExecutions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_JobPermissionEntries_JobId_SubjectType_SubjectValue",
                schema: "scheduler",
                table: "JobPermissionEntries",
                columns: new[] { "JobId", "SubjectType", "SubjectValue" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JobSchedules_IsEnabled_NextRunAtUtc",
                schema: "scheduler",
                table: "JobSchedules",
                columns: new[] { "IsEnabled", "NextRunAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_JobSchedules_JobId",
                schema: "scheduler",
                table: "JobSchedules",
                column: "JobId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JobSchedules_MisfirePolicy",
                schema: "scheduler",
                table: "JobSchedules",
                column: "MisfirePolicy");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledJobs_IsQuarantined",
                schema: "scheduler",
                table: "ScheduledJobs",
                column: "IsQuarantined");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledJobs_Name",
                schema: "scheduler",
                table: "ScheduledJobs",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledJobs_Status",
                schema: "scheduler",
                table: "ScheduledJobs",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_SchedulerLocks_ExpiresAtUtc",
                schema: "scheduler",
                table: "SchedulerLocks",
                column: "ExpiresAtUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobDependencies",
                schema: "scheduler");

            migrationBuilder.DropTable(
                name: "JobExecutions",
                schema: "scheduler");

            migrationBuilder.DropTable(
                name: "JobPermissionEntries",
                schema: "scheduler");

            migrationBuilder.DropTable(
                name: "JobSchedules",
                schema: "scheduler");

            migrationBuilder.DropTable(
                name: "SchedulerLocks",
                schema: "scheduler");

            migrationBuilder.DropTable(
                name: "ScheduledJobs",
                schema: "scheduler");
        }
    }
