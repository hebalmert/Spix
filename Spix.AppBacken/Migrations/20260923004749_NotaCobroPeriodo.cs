using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spix.AppBacken.Migrations
{
    /// <inheritdoc />
    public partial class NotaCobroPeriodo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MonthType",
                table: "CxCBills",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "YearNumber",
                table: "CxCBills",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_CxCBills_CorporationId_ContractClientId_YearNumber_MonthType",
                table: "CxCBills",
                columns: new[] { "CorporationId", "ContractClientId", "YearNumber", "MonthType" },
                unique: true,
                filter: "[Cancelled] = 0 AND [YearNumber] > 0");

            migrationBuilder.CreateIndex(
                name: "IX_CxCBills_CorporationId_YearNumber_MonthType",
                table: "CxCBills",
                columns: new[] { "CorporationId", "YearNumber", "MonthType" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CxCBills_CorporationId_ContractClientId_YearNumber_MonthType",
                table: "CxCBills");

            migrationBuilder.DropIndex(
                name: "IX_CxCBills_CorporationId_YearNumber_MonthType",
                table: "CxCBills");

            migrationBuilder.DropColumn(
                name: "MonthType",
                table: "CxCBills");

            migrationBuilder.DropColumn(
                name: "YearNumber",
                table: "CxCBills");
        }
    }
}
