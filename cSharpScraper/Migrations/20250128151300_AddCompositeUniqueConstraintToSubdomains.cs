using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cSharpScraper.Migrations
{
    /// <inheritdoc />
    public partial class AddCompositeUniqueConstraintToSubdomains : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Subdomains_Name_DomainId",
                table: "Subdomains",
                columns: new[] { "Name", "DomainId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Subdomains_Name_DomainId",
                table: "Subdomains");
        }
    }
}
