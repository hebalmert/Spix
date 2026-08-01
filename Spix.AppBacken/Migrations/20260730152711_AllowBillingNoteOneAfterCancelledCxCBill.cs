using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spix.AppBacken.Migrations
{
    /// <inheritdoc />
    public partial class AllowBillingNoteOneAfterCancelledCxCBill : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BillingNoteOnes_CorporationId_ContractClientId_YearNumber_MonthType",
                table: "BillingNoteOnes");

            migrationBuilder.CreateIndex(
                name: "IX_BillingNoteOnes_CorporationId_ContractClientId_YearNumber_MonthType",
                table: "BillingNoteOnes",
                columns: new[] { "CorporationId", "ContractClientId", "YearNumber", "MonthType" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BillingNoteOnes_CorporationId_ContractClientId_YearNumber_MonthType",
                table: "BillingNoteOnes");

            migrationBuilder.CreateIndex(
                name: "IX_BillingNoteOnes_CorporationId_ContractClientId_YearNumber_MonthType",
                table: "BillingNoteOnes",
                columns: new[] { "CorporationId", "ContractClientId", "YearNumber", "MonthType" },
                unique: true);
        }
    }
}
