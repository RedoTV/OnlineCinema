using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineCinema.Backend.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangeActorBirthDateToDateOnly : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Genres");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "BirthDate",
                table: "Actors",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "PasswordHash", "PasswordSalt" },
                values: new object[] { "bstXX+QdN7+MRavzQNwajhKd2ko=", "4QzCu2ff02Vx1Gc7xucnXQ==" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "PasswordHash", "PasswordSalt" },
                values: new object[] { "JBM9kZZk1OAux7SaVPkWcZgSqCQ=", "77vZXzleDE7hwM5UKvBwfQ==" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "PasswordHash", "PasswordSalt" },
                values: new object[] { "eyMUTbYMlnbZ9x2tR0QKgSY+2Ec=", "VqJgjY923A6m4gk+rUOGhQ==" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Genres",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "BirthDate",
                table: "Actors",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "PasswordHash", "PasswordSalt" },
                values: new object[] { "vyI768VhKmqEVZzA5SleHKGzMp8=", "dCaEDPZb904WVbg+1h4s5A==" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "PasswordHash", "PasswordSalt" },
                values: new object[] { "1WM5FiEmCB1OS3Hgy9IX0SH7sXk=", "N7MMM2TIu/uoSebFGjTpug==" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "PasswordHash", "PasswordSalt" },
                values: new object[] { "hxtQMbvjUhu1+DtzVnO/HR79B/M=", "nqBYGfLTJ7BMhn2btiSAPA==" });
        }
    }
}
