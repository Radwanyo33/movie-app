using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LiveMovies.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreatePostgreSQL : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "$2a$11$3SP2j5HjEzJh.zwymT0Fhumc2kjjJGVL.8hkUyqVkFMpX5Z7ymq3e" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 10, 30, 7, 43, 36, 324, DateTimeKind.Utc).AddTicks(3530), "$2a$11$9N7Y2hj7OvHozaVjmH.uau/1kHJlJr2F4IztzlDv3SSA2cBNBrjvq" });
        }
    }
}
