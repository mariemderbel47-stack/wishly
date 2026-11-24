using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wishly.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AjoutPhotoUrlEtDateOccasion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PhotoUrl",
                table: "Users",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOccasion",
                table: "Wishlists",
                type: "datetime(6)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhotoUrl",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DateOccasion",
                table: "Wishlists");
        }
    }
}