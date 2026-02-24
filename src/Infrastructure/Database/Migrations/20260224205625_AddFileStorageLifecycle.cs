using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
#pragma warning disable IDE0161, CA1861

namespace Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddFileStorageLifecycle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StagingObjectKey",
                schema: "files",
                table: "FileAssets",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StorageAvailableAtUtc",
                schema: "files",
                table: "FileAssets",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StorageError",
                schema: "files",
                table: "FileAssets",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StorageLastCheckedAtUtc",
                schema: "files",
                table: "FileAssets",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StorageRetryCount",
                schema: "files",
                table: "FileAssets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StorageStatus",
                schema: "files",
                table: "FileAssets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "UploadRequestedAtUtc",
                schema: "files",
                table: "FileAssets",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_FileAssets_StorageStatus_UploadRequestedAtUtc",
                schema: "files",
                table: "FileAssets",
                columns: new[] { "StorageStatus", "UploadRequestedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FileAssets_StorageStatus_UploadRequestedAtUtc",
                schema: "files",
                table: "FileAssets");

            migrationBuilder.DropColumn(
                name: "StagingObjectKey",
                schema: "files",
                table: "FileAssets");

            migrationBuilder.DropColumn(
                name: "StorageAvailableAtUtc",
                schema: "files",
                table: "FileAssets");

            migrationBuilder.DropColumn(
                name: "StorageError",
                schema: "files",
                table: "FileAssets");

            migrationBuilder.DropColumn(
                name: "StorageLastCheckedAtUtc",
                schema: "files",
                table: "FileAssets");

            migrationBuilder.DropColumn(
                name: "StorageRetryCount",
                schema: "files",
                table: "FileAssets");

            migrationBuilder.DropColumn(
                name: "StorageStatus",
                schema: "files",
                table: "FileAssets");

            migrationBuilder.DropColumn(
                name: "UploadRequestedAtUtc",
                schema: "files",
                table: "FileAssets");
        }
    }
}

#pragma warning restore IDE0161, CA1861

