using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spix.AppBacken.Migrations
{
    /// <inheritdoc />
    public partial class NewPreExoneratedDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PreExonerateds",
                columns: table => new
                {
                    PreExoneratedId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    DateExonerated = table.Column<DateTime>(type: "date", nullable: false),
                    ExoneratedControl = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: true),
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
                    table.PrimaryKey("PK_PreExonerateds", x => x.PreExoneratedId);
                    table.ForeignKey(
                        name: "FK_PreExonerateds_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "ClientId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PreExonerateds_ContractClients_ContractClientId",
                        column: x => x.ContractClientId,
                        principalTable: "ContractClients",
                        principalColumn: "ContractClientId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PreExonerateds_Corporations_CorporationId",
                        column: x => x.CorporationId,
                        principalTable: "Corporations",
                        principalColumn: "CorporationId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PreExonerateds_CxCBills_CxCBillId",
                        column: x => x.CxCBillId,
                        principalTable: "CxCBills",
                        principalColumn: "CxCBillId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PreExonerateds_Plans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "Plans",
                        principalColumn: "PlanId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PreExonerateds_ClientId",
                table: "PreExonerateds",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_PreExonerateds_ContractClientId",
                table: "PreExonerateds",
                column: "ContractClientId");

            migrationBuilder.CreateIndex(
                name: "IX_PreExonerateds_CorporationId_ContractClientId_YearNumber_MonthType",
                table: "PreExonerateds",
                columns: new[] { "CorporationId", "ContractClientId", "YearNumber", "MonthType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PreExonerateds_CxCBillId",
                table: "PreExonerateds",
                column: "CxCBillId");

            migrationBuilder.CreateIndex(
                name: "IX_PreExonerateds_PlanId",
                table: "PreExonerateds",
                column: "PlanId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PreExonerateds");
        }
    }
}
