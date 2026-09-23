using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spix.AppBacken.Migrations
{
    /// <inheritdoc />
    public partial class IndicesFacturaYNota : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Sells_CorporationId_DateSell",
                table: "Sells",
                columns: new[] { "CorporationId", "DateSell" });

            migrationBuilder.CreateIndex(
                name: "IX_CxCBills_CorporationId_DateNote",
                table: "CxCBills",
                columns: new[] { "CorporationId", "DateNote" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Sells_CorporationId_DateSell",
                table: "Sells");

            migrationBuilder.DropIndex(
                name: "IX_CxCBills_CorporationId_DateNote",
                table: "CxCBills");
        }
    }
}
