using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cSharpScraper.Migrations
{
    /// <inheritdoc />
    public partial class REmoveOldSubdomainIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Subdomains_Name",
                table: "Subdomains");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Subdomains_Name",
                table: "Subdomains",
                column: "Name",
                unique: true);
        }
    }
}
