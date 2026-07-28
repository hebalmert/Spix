using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spix.AppBacken.Migrations
{
    /// <inheritdoc />
    public partial class ContractorPaymentNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ContractorPayments_CorporationId",
                table: "ContractorPayments");

            migrationBuilder.AddColumn<string>(
                name: "PaymentNumber",
                table: "ContractorPayments",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_ContractorPayments_CorporationId_PaymentNumber",
                table: "ContractorPayments",
                columns: new[] { "CorporationId", "PaymentNumber" },
                unique: true,
                filter: "[PaymentNumber] <> ''");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ContractorPayments_CorporationId_PaymentNumber",
                table: "ContractorPayments");

            migrationBuilder.DropColumn(
                name: "PaymentNumber",
                table: "ContractorPayments");

            migrationBuilder.CreateIndex(
                name: "IX_ContractorPayments_CorporationId",
                table: "ContractorPayments",
                column: "CorporationId");
        }
    }
}
