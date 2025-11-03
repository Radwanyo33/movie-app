using System;
using Microsoft.EntityFrameworkCore.Migrations;
using System.Text.Json;

#nullable disable

namespace LiveMovies.Migrations
{
    /// <inheritdoc />
    public partial class PopulateJsonFieldsFromExistingData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 10, 30, 7, 43, 36, 324, DateTimeKind.Utc).AddTicks(3530), "$2a$11$9N7Y2hj7OvHozaVjmH.uau/1kHJlJr2F4IztzlDv3SSA2cBNBrjvq" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 10, 30, 7, 28, 39, 702, DateTimeKind.Utc).AddTicks(9266), "$2a$11$t7Vv.c4ch/Hm4tJZ.GhEeuyds8BY7L.n3QqTKnwXqizEpwc88YiGK" });
        }
    }
}
