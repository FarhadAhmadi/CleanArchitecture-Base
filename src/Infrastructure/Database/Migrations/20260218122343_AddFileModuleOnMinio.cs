using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
#pragma warning disable IDE0161, CA1861

namespace Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddFileModuleOnMinio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FileAssets",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OwnerUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ObjectKey = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    Extension = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    Module = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Folder = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Sha256 = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    IsScanned = table.Column<bool>(type: "bit", nullable: false),
                    IsInfected = table.Column<bool>(type: "bit", nullable: false),
                    IsEncrypted = table.Column<bool>(type: "bit", nullable: false),
                    UploadedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileAssets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FileAssets_Users_OwnerUserId",
                        column: x => x.OwnerUserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FileAccessAudits",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Action = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    TimestampUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileAccessAudits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FileAccessAudits_FileAssets_FileId",
                        column: x => x.FileId,
                        principalSchema: "dbo",
                        principalTable: "FileAssets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FilePermissionEntries",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubjectType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SubjectValue = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CanRead = table.Column<bool>(type: "bit", nullable: false),
                    CanWrite = table.Column<bool>(type: "bit", nullable: false),
                    CanDelete = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FilePermissionEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FilePermissionEntries_FileAssets_FileId",
                        column: x => x.FileId,
                        principalSchema: "dbo",
                        principalTable: "FileAssets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FileTags",
                schema: "dbo",
                columns: table => new
                {
                    FileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Tag = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileTags", x => new { x.FileId, x.Tag });
                    table.ForeignKey(
                        name: "FK_FileTags_FileAssets_FileId",
                        column: x => x.FileId,
                        principalSchema: "dbo",
                        principalTable: "FileAssets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FileAccessAudits_FileId_TimestampUtc",
                schema: "dbo",
                table: "FileAccessAudits",
                columns: new[] { "FileId", "TimestampUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_FileAccessAudits_UserId",
                schema: "dbo",
                table: "FileAccessAudits",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_FileAssets_FileName",
                schema: "dbo",
                table: "FileAssets",
                column: "FileName");

            migrationBuilder.CreateIndex(
                name: "IX_FileAssets_IsDeleted",
                schema: "dbo",
                table: "FileAssets",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_FileAssets_Module_UploadedAtUtc",
                schema: "dbo",
                table: "FileAssets",
                columns: new[] { "Module", "UploadedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_FileAssets_ObjectKey",
                schema: "dbo",
                table: "FileAssets",
                column: "ObjectKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FileAssets_OwnerUserId",
                schema: "dbo",
                table: "FileAssets",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_FilePermissionEntries_FileId_SubjectType_SubjectValue",
                schema: "dbo",
                table: "FilePermissionEntries",
                columns: new[] { "FileId", "SubjectType", "SubjectValue" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FileTags_Tag",
                schema: "dbo",
                table: "FileTags",
                column: "Tag");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FileAccessAudits",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "FilePermissionEntries",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "FileTags",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "FileAssets",
                schema: "dbo");
        }
    }
}
#pragma warning restore IDE0161, CA1861
