using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spix.AppBacken.Migrations
{
    /// <inheritdoc />
    public partial class ReactivacionPorPago : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DatePaymentReceived",
                table: "ContractSuspendeds",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PaymentReceived",
                table: "ContractSuspendeds",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_ContractSuspendeds_CorporationId_PaymentReceived_DateReactivated",
                table: "ContractSuspendeds",
                columns: new[] { "CorporationId", "PaymentReceived", "DateReactivated" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ContractSuspendeds_CorporationId_PaymentReceived_DateReactivated",
                table: "ContractSuspendeds");

            migrationBuilder.DropColumn(
                name: "DatePaymentReceived",
                table: "ContractSuspendeds");

            migrationBuilder.DropColumn(
                name: "PaymentReceived",
                table: "ContractSuspendeds");
        }
    }
}
