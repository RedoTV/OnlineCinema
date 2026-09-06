using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace OnlineCinema.Backend.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserWatches : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserWatches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    MovieId = table.Column<int>(type: "integer", nullable: true),
                    EpisodeId = table.Column<int>(type: "integer", nullable: true),
                    WatchedSeconds = table.Column<double>(type: "double precision", nullable: false),
                    WatchedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserWatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserWatches_Episodes_EpisodeId",
                        column: x => x.EpisodeId,
                        principalTable: "Episodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserWatches_Movies_MovieId",
                        column: x => x.MovieId,
                        principalTable: "Movies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserWatches_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_UserWatches_EpisodeId",
                table: "UserWatches",
                column: "EpisodeId");

            migrationBuilder.CreateIndex(
                name: "IX_UserWatches_MovieId_WatchedAt",
                table: "UserWatches",
                columns: new[] { "MovieId", "WatchedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_UserWatches_UserId",
                table: "UserWatches",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserWatches_WatchedAt",
                table: "UserWatches",
                column: "WatchedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserWatches");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "PasswordHash", "PasswordSalt" },
                values: new object[] { "3xkwEz+M7Yxylkod5FA+cDIGb9INwZI1Krp/yrCu78Y=", "qffpP2YQnnig4ao9uFmaTg==" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "PasswordHash", "PasswordSalt" },
                values: new object[] { "xUitPH8eWe5f3h9EbpTlwbwuvuFHNPLzE+M4fznHcq0=", "lof/Tvbn1+jnvTn02pveBg==" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "PasswordHash", "PasswordSalt" },
                values: new object[] { "Hs+jQ73II0Daa2QUxvAW3aGm+Zxkeykxt/b+wNR0j8M=", "jKTYIczCVY3asoFwewq++A==" });
        }
    }
}
