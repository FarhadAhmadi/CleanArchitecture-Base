using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Database.Migrations;

public partial class AddProfileMediaAndMusic : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "FavoriteMusicArtist",
            schema: "profiles",
            table: "UserProfiles",
            type: "nvarchar(200)",
            maxLength: 200,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "FavoriteMusicLink",
            schema: "profiles",
            table: "UserProfiles",
            type: "nvarchar(800)",
            maxLength: 800,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "FavoriteMusicTitle",
            schema: "profiles",
            table: "UserProfiles",
            type: "nvarchar(200)",
            maxLength: 200,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "FavoritePlaylistLink",
            schema: "profiles",
            table: "UserProfiles",
            type: "nvarchar(800)",
            maxLength: 800,
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "FavoriteMusicArtist",
            schema: "profiles",
            table: "UserProfiles");

        migrationBuilder.DropColumn(
            name: "FavoriteMusicLink",
            schema: "profiles",
            table: "UserProfiles");

        migrationBuilder.DropColumn(
            name: "FavoriteMusicTitle",
            schema: "profiles",
            table: "UserProfiles");

        migrationBuilder.DropColumn(
            name: "FavoritePlaylistLink",
            schema: "profiles",
            table: "UserProfiles");
    }
}
