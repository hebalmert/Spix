using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spix.AppBacken.Migrations
{
    /// <inheritdoc />
    public partial class IndicesFacturaNotaYContratista : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ContractorAccountPayables_CorporationId_Paid_DateCreated",
                table: "ContractorAccountPayables",
                columns: new[] { "CorporationId", "Paid", "DateCreated" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ContractorAccountPayables_CorporationId_Paid_DateCreated",
                table: "ContractorAccountPayables");
        }
    }
}
