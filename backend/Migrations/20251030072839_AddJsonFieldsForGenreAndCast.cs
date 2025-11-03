using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LiveMovies.Migrations
{
    /// <inheritdoc />
    public partial class AddJsonFieldsForGenreAndCast : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CastJson",
                table: "Movies",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "GenreJson",
                table: "Movies",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 10, 30, 7, 28, 39, 702, DateTimeKind.Utc).AddTicks(9266), "$2a$11$t7Vv.c4ch/Hm4tJZ.GhEeuyds8BY7L.n3QqTKnwXqizEpwc88YiGK" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CastJson",
                table: "Movies");

            migrationBuilder.DropColumn(
                name: "GenreJson",
                table: "Movies");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 10, 28, 11, 11, 34, 191, DateTimeKind.Utc).AddTicks(42), "$2a$11$NpZX.kWEMgApyff2ZQGUreFJcPoax9.v97/2ubdfYOj7Q27lk01.e" });
        }
    }
}
