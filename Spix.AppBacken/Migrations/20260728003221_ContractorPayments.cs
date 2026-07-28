using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spix.AppBacken.Migrations
{
    /// <inheritdoc />
    public partial class ContractorPayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContractorAccountPayables",
                columns: table => new
                {
                    ContractorAccountPayableId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    DateCreated = table.Column<DateTime>(type: "date", nullable: false),
                    ContractorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContractClientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CxCBillId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CxCBillDetailId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    BaseAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Balance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Paid = table.Column<bool>(type: "bit", nullable: false),
                    DatePaid = table.Column<DateTime>(type: "date", nullable: true),
                    CorporationId = table.Column<int>(type: "int", nullable: false),
                    UsuarioOwner = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractorAccountPayables", x => x.ContractorAccountPayableId);
                    table.ForeignKey(
                        name: "FK_ContractorAccountPayables_ContractClients_ContractClientId",
                        column: x => x.ContractClientId,
                        principalTable: "ContractClients",
                        principalColumn: "ContractClientId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContractorAccountPayables_Contractors_ContractorId",
                        column: x => x.ContractorId,
                        principalTable: "Contractors",
                        principalColumn: "ContractorId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContractorAccountPayables_Corporations_CorporationId",
                        column: x => x.CorporationId,
                        principalTable: "Corporations",
                        principalColumn: "CorporationId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContractorAccountPayables_CxCBillDetails_CxCBillDetailId",
                        column: x => x.CxCBillDetailId,
                        principalTable: "CxCBillDetails",
                        principalColumn: "CxCBillDetailId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContractorAccountPayables_CxCBills_CxCBillId",
                        column: x => x.CxCBillId,
                        principalTable: "CxCBills",
                        principalColumn: "CxCBillId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ContractorPayments",
                columns: table => new
                {
                    ContractorPaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    DatePayment = table.Column<DateTime>(type: "date", nullable: false),
                    ContractorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentMode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Reference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Detail = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CorporationId = table.Column<int>(type: "int", nullable: false),
                    UsuarioOwner = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractorPayments", x => x.ContractorPaymentId);
                    table.ForeignKey(
                        name: "FK_ContractorPayments_Contractors_ContractorId",
                        column: x => x.ContractorId,
                        principalTable: "Contractors",
                        principalColumn: "ContractorId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContractorPayments_Corporations_CorporationId",
                        column: x => x.CorporationId,
                        principalTable: "Corporations",
                        principalColumn: "CorporationId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ContractorPaymentDetails",
                columns: table => new
                {
                    ContractorPaymentDetailId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    ContractorPaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContractorAccountPayableId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Payment = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractorPaymentDetails", x => x.ContractorPaymentDetailId);
                    table.ForeignKey(
                        name: "FK_ContractorPaymentDetails_ContractorAccountPayables_ContractorAccountPayableId",
                        column: x => x.ContractorAccountPayableId,
                        principalTable: "ContractorAccountPayables",
                        principalColumn: "ContractorAccountPayableId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContractorPaymentDetails_ContractorPayments_ContractorPaymentId",
                        column: x => x.ContractorPaymentId,
                        principalTable: "ContractorPayments",
                        principalColumn: "ContractorPaymentId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContractorAccountPayables_ContractClientId",
                table: "ContractorAccountPayables",
                column: "ContractClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractorAccountPayables_ContractorId",
                table: "ContractorAccountPayables",
                column: "ContractorId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractorAccountPayables_CorporationId_CxCBillDetailId",
                table: "ContractorAccountPayables",
                columns: new[] { "CorporationId", "CxCBillDetailId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContractorAccountPayables_CxCBillDetailId",
                table: "ContractorAccountPayables",
                column: "CxCBillDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractorAccountPayables_CxCBillId",
                table: "ContractorAccountPayables",
                column: "CxCBillId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractorPaymentDetails_ContractorAccountPayableId",
                table: "ContractorPaymentDetails",
                column: "ContractorAccountPayableId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractorPaymentDetails_ContractorPaymentId_ContractorAccountPayableId",
                table: "ContractorPaymentDetails",
                columns: new[] { "ContractorPaymentId", "ContractorAccountPayableId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContractorPayments_ContractorId",
                table: "ContractorPayments",
                column: "ContractorId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractorPayments_CorporationId",
                table: "ContractorPayments",
                column: "CorporationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContractorPaymentDetails");

            migrationBuilder.DropTable(
                name: "ContractorAccountPayables");

            migrationBuilder.DropTable(
                name: "ContractorPayments");
        }
    }
}
