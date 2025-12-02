using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineCinema.Backend.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveReviewFromRating : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Review",
                table: "Ratings");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "PasswordHash", "PasswordSalt" },
                values: new object[] { "xsv+d7xzxiGzAdGbbcLfHEE02RE=", "m4srpMDEA0fOGKbS7Kvj7A==" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "PasswordHash", "PasswordSalt" },
                values: new object[] { "xBufb4AHJSN/ULsQWqaIK92KWc0=", "Oic1Keqplc8K7yeRYIFwqQ==" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "PasswordHash", "PasswordSalt" },
                values: new object[] { "1O6BAFjP65a/1B4uUKL2p3BWw6c=", "xfUEc/UtoRtw5Q+weiRdTw==" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Review",
                table: "Ratings",
                type: "text",
                nullable: true);

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
    }
}
