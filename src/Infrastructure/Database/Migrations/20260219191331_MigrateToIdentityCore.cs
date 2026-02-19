using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable IDE0161

namespace Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class MigrateToIdentityCore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Email",
                schema: "users",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Roles_Name",
                schema: "auth",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "LockoutEndUtc",
                schema: "users",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "FailedLoginCount",
                schema: "users",
                table: "Users",
                newName: "AccessFailedCount");

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                schema: "users",
                table: "Users",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(4000)",
                oldMaxLength: 4000);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                schema: "users",
                table: "Users",
                type: "nvarchar(320)",
                maxLength: 320,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(320)",
                oldMaxLength: 320);

            migrationBuilder.AddColumn<string>(
                name: "ConcurrencyStamp",
                schema: "users",
                table: "Users",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EmailConfirmed",
                schema: "users",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "LockoutEnabled",
                schema: "users",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LockoutEnd",
                schema: "users",
                table: "Users",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NormalizedEmail",
                schema: "users",
                table: "Users",
                type: "nvarchar(320)",
                maxLength: 320,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NormalizedUserName",
                schema: "users",
                table: "Users",
                type: "nvarchar(320)",
                maxLength: 320,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                schema: "users",
                table: "Users",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PhoneNumberConfirmed",
                schema: "users",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SecurityStamp",
                schema: "users",
                table: "Users",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TwoFactorEnabled",
                schema: "users",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UserName",
                schema: "users",
                table: "Users",
                type: "nvarchar(320)",
                maxLength: 320,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "auth",
                table: "Roles",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "ConcurrencyStamp",
                schema: "auth",
                table: "Roles",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NormalizedName",
                schema: "auth",
                table: "Roles",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RoleClaims",
                schema: "auth",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleClaims_Roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "auth",
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserClaims",
                schema: "auth",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserClaims_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "users",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserLogins",
                schema: "auth",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_UserLogins_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "users",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserTokens",
                schema: "auth",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_UserTokens_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "users",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                schema: "users",
                table: "Users",
                column: "NormalizedEmail",
                unique: true,
                filter: "[NormalizedEmail] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                schema: "users",
                table: "Users",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                schema: "auth",
                table: "Roles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_RoleClaims_RoleId",
                schema: "auth",
                table: "RoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserClaims_UserId",
                schema: "auth",
                table: "UserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLogins_UserId",
                schema: "auth",
                table: "UserLogins",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoleClaims",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "UserClaims",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "UserLogins",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "UserTokens",
                schema: "auth");

            migrationBuilder.DropIndex(
                name: "EmailIndex",
                schema: "users",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "UserNameIndex",
                schema: "users",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "RoleNameIndex",
                schema: "auth",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "ConcurrencyStamp",
                schema: "users",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "EmailConfirmed",
                schema: "users",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LockoutEnabled",
                schema: "users",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LockoutEnd",
                schema: "users",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "NormalizedEmail",
                schema: "users",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "NormalizedUserName",
                schema: "users",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                schema: "users",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PhoneNumberConfirmed",
                schema: "users",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SecurityStamp",
                schema: "users",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TwoFactorEnabled",
                schema: "users",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UserName",
                schema: "users",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ConcurrencyStamp",
                schema: "auth",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "NormalizedName",
                schema: "auth",
                table: "Roles");

            migrationBuilder.RenameColumn(
                name: "AccessFailedCount",
                schema: "users",
                table: "Users",
                newName: "FailedLoginCount");

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                schema: "users",
                table: "Users",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                schema: "users",
                table: "Users",
                type: "nvarchar(320)",
                maxLength: 320,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(320)",
                oldMaxLength: 320,
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LockoutEndUtc",
                schema: "users",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "auth",
                table: "Roles",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                schema: "users",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Name",
                schema: "auth",
                table: "Roles",
                column: "Name",
                unique: true);
        }
    }
}

#pragma warning restore IDE0161
