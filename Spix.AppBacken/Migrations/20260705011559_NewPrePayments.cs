using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spix.AppBacken.Migrations
{
    /// <inheritdoc />
    public partial class NewPrePayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PrePayments",
                columns: table => new
                {
                    PrePaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    DatePayment = table.Column<DateTime>(type: "date", nullable: false),
                    PaymentControl = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: true),
                    ClientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContractClientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    YearNumber = table.Column<int>(type: "int", nullable: false),
                    MonthType = table.Column<int>(type: "int", nullable: false),
                    TaxRate = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PriceWithTax = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Billed = table.Column<bool>(type: "bit", nullable: false),
                    DateBilled = table.Column<DateTime>(type: "date", nullable: true),
                    CxCBillId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CorporationId = table.Column<int>(type: "int", nullable: false),
                    UsuarioOwner = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrePayments", x => x.PrePaymentId);
                    table.ForeignKey(
                        name: "FK_PrePayments_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "ClientId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PrePayments_ContractClients_ContractClientId",
                        column: x => x.ContractClientId,
                        principalTable: "ContractClients",
                        principalColumn: "ContractClientId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PrePayments_Corporations_CorporationId",
                        column: x => x.CorporationId,
                        principalTable: "Corporations",
                        principalColumn: "CorporationId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PrePayments_CxCBills_CxCBillId",
                        column: x => x.CxCBillId,
                        principalTable: "CxCBills",
                        principalColumn: "CxCBillId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PrePayments_Plans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "Plans",
                        principalColumn: "PlanId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PrePayments_ClientId",
                table: "PrePayments",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_PrePayments_ContractClientId",
                table: "PrePayments",
                column: "ContractClientId");

            migrationBuilder.CreateIndex(
                name: "IX_PrePayments_CorporationId_ContractClientId_YearNumber_MonthType",
                table: "PrePayments",
                columns: new[] { "CorporationId", "ContractClientId", "YearNumber", "MonthType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PrePayments_CxCBillId",
                table: "PrePayments",
                column: "CxCBillId");

            migrationBuilder.CreateIndex(
                name: "IX_PrePayments_PlanId",
                table: "PrePayments",
                column: "PlanId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PrePayments");
        }
    }
}
