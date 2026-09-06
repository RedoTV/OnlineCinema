using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineCinema.Backend.Data.Migrations
{
    /// <inheritdoc />
    public partial class GuestWatches : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "UserWatches",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "ViewerKey",
                table: "UserWatches",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "PasswordHash", "PasswordSalt" },
                values: new object[] { "jbGFC/NM5KyGeDWH5rZ4UVc1nz60zEdy/pwUQ/6R4zg=", "ZKFfsP7SD3wcVK3ABwZ5Hg==" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "PasswordHash", "PasswordSalt" },
                values: new object[] { "SHhgE02c+x3pOdDaanz3550Q4sN9wGjhMzg4eHc5/Oo=", "cLHyCRLdyh1tiDeiAU0WaQ==" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "PasswordHash", "PasswordSalt" },
                values: new object[] { "I9zDoUurCiLlZYv3vOFZ6U9X63h8VC7Iglc7OJrPtEE=", "MbUZhFSJ7kOA0SK4IL2J5A==" });

            migrationBuilder.CreateIndex(
                name: "IX_UserWatches_ViewerKey",
                table: "UserWatches",
                column: "ViewerKey");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserWatches_ViewerKey",
                table: "UserWatches");

            migrationBuilder.DropColumn(
                name: "ViewerKey",
                table: "UserWatches");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "UserWatches",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "PasswordHash", "PasswordSalt" },
                values: new object[] { "wPhA7IYa4xElxYe3dI/c7/ZFNhTHyhePs3sgGjmNfKo=", "i5gMDayxgpyfsp54ZTedaQ==" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "PasswordHash", "PasswordSalt" },
                values: new object[] { "u4YjZOU33SgrO1uOYZbviH3CiDzgYDmQMLtxFMVNrI4=", "GtqXUtZNiA9UZdKwJ9Jh7w==" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "PasswordHash", "PasswordSalt" },
                values: new object[] { "QayURroy+JFaaQ86bdgYlWAYL+xf5nyMqpZAz2qPLLk=", "Z+f0eS1S6JPOAGiuqFSHBQ==" });
        }
    }
}
