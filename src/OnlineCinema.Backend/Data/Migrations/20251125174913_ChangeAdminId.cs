using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace OnlineCinema.Backend.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangeAdminId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "FirstName", "LastName", "PasswordHash", "PasswordSalt", "Role", "UpdatedAt", "Username" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 11, 22, 20, 22, 53, 0, DateTimeKind.Utc), "admin1@onlinecinema.com", "Admin", "One", "vyI768VhKmqEVZzA5SleHKGzMp8=", "dCaEDPZb904WVbg+1h4s5A==", "Admin", new DateTime(2025, 11, 22, 20, 22, 53, 0, DateTimeKind.Utc), "admin1" },
                    { 2, new DateTime(2025, 11, 22, 20, 22, 53, 0, DateTimeKind.Utc), "admin2@onlinecinema.com", "Admin", "Two", "1WM5FiEmCB1OS3Hgy9IX0SH7sXk=", "N7MMM2TIu/uoSebFGjTpug==", "Admin", new DateTime(2025, 11, 22, 20, 22, 53, 0, DateTimeKind.Utc), "admin2" },
                    { 3, new DateTime(2025, 11, 22, 20, 22, 53, 0, DateTimeKind.Utc), "admin3@onlinecinema.com", "Admin", "Three", "hxtQMbvjUhu1+DtzVnO/HR79B/M=", "nqBYGfLTJ7BMhn2btiSAPA==", "Admin", new DateTime(2025, 11, 22, 20, 22, 53, 0, DateTimeKind.Utc), "admin3" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3);

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
    }
}
