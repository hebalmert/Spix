using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spix.AppBacken.Migrations
{
    /// <inheritdoc />
    public partial class PrePaymentDetalle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PrePaymentDetails",
                columns: table => new
                {
                    PrePaymentDetailId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    PrePaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LineType = table.Column<int>(type: "int", nullable: false),
                    Concept = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    PlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ServiceRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ServiceRequestDetailId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TaxRate = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PriceWithTax = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CorporationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrePaymentDetails", x => x.PrePaymentDetailId);
                    table.ForeignKey(
                        name: "FK_PrePaymentDetails_Corporations_CorporationId",
                        column: x => x.CorporationId,
                        principalTable: "Corporations",
                        principalColumn: "CorporationId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PrePaymentDetails_PrePayments_PrePaymentId",
                        column: x => x.PrePaymentId,
                        principalTable: "PrePayments",
                        principalColumn: "PrePaymentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PrePaymentDetails_ServiceRequests_ServiceRequestId",
                        column: x => x.ServiceRequestId,
                        principalTable: "ServiceRequests",
                        principalColumn: "ServiceRequestId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PrePaymentDetails_CorporationId_ServiceRequestDetailId",
                table: "PrePaymentDetails",
                columns: new[] { "CorporationId", "ServiceRequestDetailId" },
                unique: true,
                filter: "[ServiceRequestDetailId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PrePaymentDetails_PrePaymentId",
                table: "PrePaymentDetails",
                column: "PrePaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_PrePaymentDetails_ServiceRequestId",
                table: "PrePaymentDetails",
                column: "ServiceRequestId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PrePaymentDetails");
        }
    }
}
