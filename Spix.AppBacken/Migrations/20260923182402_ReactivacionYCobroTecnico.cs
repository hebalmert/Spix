using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spix.AppBacken.Migrations
{
    /// <inheritdoc />
    public partial class ReactivacionYCobroTecnico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CxCBillDetails_CorporationId",
                table: "CxCBillDetails");

            migrationBuilder.CreateIndex(
                name: "IX_CxCBillDetails_CorporationId_UserId_DatePayment",
                table: "CxCBillDetails",
                columns: new[] { "CorporationId", "UserId", "DatePayment" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CxCBillDetails_CorporationId_UserId_DatePayment",
                table: "CxCBillDetails");

            migrationBuilder.CreateIndex(
                name: "IX_CxCBillDetails_CorporationId",
                table: "CxCBillDetails",
                column: "CorporationId");
        }
    }
}
