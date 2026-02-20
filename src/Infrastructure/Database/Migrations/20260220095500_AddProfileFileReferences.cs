using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Database.Migrations;

public partial class AddProfileFileReferences : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "AvatarFileId",
            schema: "profiles",
            table: "UserProfiles",
            type: "uniqueidentifier",
            nullable: true);

        migrationBuilder.AddColumn<Guid>(
            name: "FavoriteMusicFileId",
            schema: "profiles",
            table: "UserProfiles",
            type: "uniqueidentifier",
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "AvatarFileId",
            schema: "profiles",
            table: "UserProfiles");

        migrationBuilder.DropColumn(
            name: "FavoriteMusicFileId",
            schema: "profiles",
            table: "UserProfiles");
    }
}
