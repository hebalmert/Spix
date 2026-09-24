using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spix.AppBacken.Migrations
{
    /// <inheritdoc />
    public partial class IndiceFacturaCliente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Sells_CorporationId_ClientId_DateSell",
                table: "Sells",
                columns: new[] { "CorporationId", "ClientId", "DateSell" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Sells_CorporationId_ClientId_DateSell",
                table: "Sells");
        }
    }
}
