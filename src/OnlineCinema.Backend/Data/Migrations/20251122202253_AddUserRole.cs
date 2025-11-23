using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace OnlineCinema.Backend.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "Users",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "User");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "FirstName", "LastName", "PasswordHash", "PasswordSalt", "Role", "UpdatedAt", "Username" },
                values: new object[,]
                {
                    { -3, new DateTime(2025, 11, 22, 20, 22, 53, 417, DateTimeKind.Utc).AddTicks(9921), "admin3@onlinecinema.com", "Admin", "Three", "06vAhswYjwulaUkcD6O3WaYQ0OY=", "aA25ICfCVWI8xdsKyGopIA==", "Admin", new DateTime(2025, 11, 22, 20, 22, 53, 417, DateTimeKind.Utc).AddTicks(9921), "admin3" },
                    { -2, new DateTime(2025, 11, 22, 20, 22, 53, 415, DateTimeKind.Utc).AddTicks(8611), "admin2@onlinecinema.com", "Admin", "Two", "YAXFZTbgP7WqMudhsmLBA4oJJlg=", "FWRX8JDy2j6ZWcPFgENLoA==", "Admin", new DateTime(2025, 11, 22, 20, 22, 53, 415, DateTimeKind.Utc).AddTicks(8611), "admin2" },
                    { -1, new DateTime(2025, 11, 22, 20, 22, 53, 413, DateTimeKind.Utc).AddTicks(7520), "admin1@onlinecinema.com", "Admin", "One", "UzbjLlilG9gOWybmPf7xNM6FMHM=", "jN8gfEid7QyZ81ckmLGdvQ==", "Admin", new DateTime(2025, 11, 22, 20, 22, 53, 413, DateTimeKind.Utc).AddTicks(7521), "admin1" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: -3);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: -2);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: -1);

            migrationBuilder.DropColumn(
                name: "Role",
                table: "Users");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");
        }
    }
}
