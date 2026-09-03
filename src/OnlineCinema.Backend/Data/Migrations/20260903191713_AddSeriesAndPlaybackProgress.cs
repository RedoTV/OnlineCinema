using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace OnlineCinema.Backend.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSeriesAndPlaybackProgress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "MovieId",
                table: "Ratings",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "SeriesId",
                table: "Ratings",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Series",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    ReleaseYear = table.Column<int>(type: "integer", nullable: true),
                    PosterUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PosterLocalPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Series", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Seasons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SeriesId = table.Column<int>(type: "integer", nullable: false),
                    SeasonNumber = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ReleaseYear = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Seasons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Seasons_Series_SeriesId",
                        column: x => x.SeriesId,
                        principalTable: "Series",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SeriesActors",
                columns: table => new
                {
                    ActorsId = table.Column<int>(type: "integer", nullable: false),
                    SeriesId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeriesActors", x => new { x.ActorsId, x.SeriesId });
                    table.ForeignKey(
                        name: "FK_SeriesActors_Actors_ActorsId",
                        column: x => x.ActorsId,
                        principalTable: "Actors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SeriesActors_Series_SeriesId",
                        column: x => x.SeriesId,
                        principalTable: "Series",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SeriesGenres",
                columns: table => new
                {
                    GenresId = table.Column<int>(type: "integer", nullable: false),
                    SeriesId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeriesGenres", x => new { x.GenresId, x.SeriesId });
                    table.ForeignKey(
                        name: "FK_SeriesGenres_Genres_GenresId",
                        column: x => x.GenresId,
                        principalTable: "Genres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SeriesGenres_Series_SeriesId",
                        column: x => x.SeriesId,
                        principalTable: "Series",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Episodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SeasonId = table.Column<int>(type: "integer", nullable: false),
                    EpisodeNumber = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Duration = table.Column<int>(type: "integer", nullable: true),
                    VideoUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    VideoLocalPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ThumbnailUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Episodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Episodes_Seasons_SeasonId",
                        column: x => x.SeasonId,
                        principalTable: "Seasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlaybackProgresses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    MovieId = table.Column<int>(type: "integer", nullable: true),
                    EpisodeId = table.Column<int>(type: "integer", nullable: true),
                    PositionSeconds = table.Column<double>(type: "double precision", nullable: false),
                    DurationSeconds = table.Column<double>(type: "double precision", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlaybackProgresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlaybackProgresses_Episodes_EpisodeId",
                        column: x => x.EpisodeId,
                        principalTable: "Episodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlaybackProgresses_Movies_MovieId",
                        column: x => x.MovieId,
                        principalTable: "Movies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlaybackProgresses_Users_UserId",
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
                values: new object[] { "Te+rgjhYbwlQUc6uOS1ziatvHZshb0BiYqmM5lOc3QU=", "SeHRXG4HvN7LtoGD0qE+4w==" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "PasswordHash", "PasswordSalt" },
                values: new object[] { "OWjq+uanm9c8LBmdMygZN5Cv/2iS/DPqbEDkgE03x0E=", "WvAzY/uWenrEYndBvC+SeA==" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "PasswordHash", "PasswordSalt" },
                values: new object[] { "dlUjtFvWmb4WkBTA3wCXft5kpmdd5EQv5u8xdYteZ+8=", "FPmHZEea9l2Pcljpwg38NA==" });

            migrationBuilder.CreateIndex(
                name: "IX_Ratings_SeriesId",
                table: "Ratings",
                column: "SeriesId");

            migrationBuilder.CreateIndex(
                name: "IX_Ratings_UserId_SeriesId",
                table: "Ratings",
                columns: new[] { "UserId", "SeriesId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Episodes_SeasonId_EpisodeNumber",
                table: "Episodes",
                columns: new[] { "SeasonId", "EpisodeNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlaybackProgresses_EpisodeId",
                table: "PlaybackProgresses",
                column: "EpisodeId");

            migrationBuilder.CreateIndex(
                name: "IX_PlaybackProgresses_MovieId",
                table: "PlaybackProgresses",
                column: "MovieId");

            migrationBuilder.CreateIndex(
                name: "IX_PlaybackProgresses_UserId_EpisodeId",
                table: "PlaybackProgresses",
                columns: new[] { "UserId", "EpisodeId" });

            migrationBuilder.CreateIndex(
                name: "IX_PlaybackProgresses_UserId_MovieId",
                table: "PlaybackProgresses",
                columns: new[] { "UserId", "MovieId" });

            migrationBuilder.CreateIndex(
                name: "IX_Seasons_SeriesId_SeasonNumber",
                table: "Seasons",
                columns: new[] { "SeriesId", "SeasonNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Series_Title",
                table: "Series",
                column: "Title");

            migrationBuilder.CreateIndex(
                name: "IX_SeriesActors_SeriesId",
                table: "SeriesActors",
                column: "SeriesId");

            migrationBuilder.CreateIndex(
                name: "IX_SeriesGenres_SeriesId",
                table: "SeriesGenres",
                column: "SeriesId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ratings_Series_SeriesId",
                table: "Ratings",
                column: "SeriesId",
                principalTable: "Series",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ratings_Series_SeriesId",
                table: "Ratings");

            migrationBuilder.DropTable(
                name: "PlaybackProgresses");

            migrationBuilder.DropTable(
                name: "SeriesActors");

            migrationBuilder.DropTable(
                name: "SeriesGenres");

            migrationBuilder.DropTable(
                name: "Episodes");

            migrationBuilder.DropTable(
                name: "Seasons");

            migrationBuilder.DropTable(
                name: "Series");

            migrationBuilder.DropIndex(
                name: "IX_Ratings_SeriesId",
                table: "Ratings");

            migrationBuilder.DropIndex(
                name: "IX_Ratings_UserId_SeriesId",
                table: "Ratings");

            migrationBuilder.DropColumn(
                name: "SeriesId",
                table: "Ratings");

            migrationBuilder.AlterColumn<int>(
                name: "MovieId",
                table: "Ratings",
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
    }
}
