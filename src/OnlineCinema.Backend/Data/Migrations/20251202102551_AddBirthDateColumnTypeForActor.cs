using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineCinema.Backend.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBirthDateColumnTypeForActor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "PasswordHash", "PasswordSalt" },
                values: new object[] { "IvCskRsQjcHeZhLMJC2YuSmUE+Y=", "tJFAYqKvn9CDcJKyVHKMog==" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "PasswordHash", "PasswordSalt" },
                values: new object[] { "R8c3K64sjrHVYfnX1pbLeHYDo0U=", "qQARiNAbOYYsgjFAvqJ8TA==" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "PasswordHash", "PasswordSalt" },
                values: new object[] { "7OoJifGJtE92lxJBOJmUlQxTNUo=", "/NPoOp0TjuLg6U1MGDLklQ==" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
    }
}
