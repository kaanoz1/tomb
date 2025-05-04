using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace tomb.Migrations
{
    /// <inheritdoc />
    public partial class third : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Caches",
                table: "Caches");

            migrationBuilder.RenameTable(
                name: "Caches",
                newName: "cache");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastActive",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_cache",
                table: "cache",
                column: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_cache",
                table: "cache");

            migrationBuilder.RenameTable(
                name: "cache",
                newName: "Caches");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastActive",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Caches",
                table: "Caches",
                column: "id");
        }
    }
}
