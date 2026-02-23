using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class AddFileStorageDeletionTracking : Migration
{
    private static readonly string[] FileCleanupIndexColumns = ["IsDeleted", "StorageDeletedAtUtc", "DeletedAtUtc"];

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTime>(
            name: "StorageDeletedAtUtc",
            schema: "files",
            table: "FileAssets",
            type: "datetime2",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_FileAssets_IsDeleted_StorageDeletedAtUtc_DeletedAtUtc",
            schema: "files",
            table: "FileAssets",
            columns: FileCleanupIndexColumns);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_FileAssets_IsDeleted_StorageDeletedAtUtc_DeletedAtUtc",
            schema: "files",
            table: "FileAssets");

        migrationBuilder.DropColumn(
            name: "StorageDeletedAtUtc",
            schema: "files",
            table: "FileAssets");
    }
}
