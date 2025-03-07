using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cSharpScraper.Migrations
{
    /// <inheritdoc />
    public partial class AddNewPropertiesToPages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasBeenScanned",
                table: "Pages",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasCompoundUrl",
                table: "Pages",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasBeenScanned",
                table: "Pages");

            migrationBuilder.DropColumn(
                name: "HasCompoundUrl",
                table: "Pages");
        }
    }
}
