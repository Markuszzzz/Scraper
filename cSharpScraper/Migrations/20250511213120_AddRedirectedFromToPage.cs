using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cSharpScraper.Migrations
{
    /// <inheritdoc />
    public partial class AddRedirectedFromToPage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RedirectedFrom",
                table: "Pages",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RedirectedFrom",
                table: "Pages");
        }
    }
}
