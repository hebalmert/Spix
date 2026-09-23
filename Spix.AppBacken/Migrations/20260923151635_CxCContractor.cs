using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spix.AppBacken.Migrations
{
    /// <inheritdoc />
    public partial class CxCContractor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CxCContractorId",
                table: "ContractorAccountPayables",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CxCContractors",
                columns: table => new
                {
                    CxCContractorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    DateNote = table.Column<DateTime>(type: "date", nullable: false),
                    NoteNumber = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: true),
                    ContractorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Balance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Paid = table.Column<bool>(type: "bit", nullable: false),
                    DatePaid = table.Column<DateTime>(type: "date", nullable: true),
                    Cancelled = table.Column<bool>(type: "bit", nullable: false),
                    DateCancelled = table.Column<DateTime>(type: "date", nullable: true),
                    DescriptionCancelled = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    CorporationId = table.Column<int>(type: "int", nullable: false),
                    UsuarioOwner = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CxCContractors", x => x.CxCContractorId);
                    table.ForeignKey(
                        name: "FK_CxCContractors_Contractors_ContractorId",
                        column: x => x.ContractorId,
                        principalTable: "Contractors",
                        principalColumn: "ContractorId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CxCContractors_Corporations_CorporationId",
                        column: x => x.CorporationId,
                        principalTable: "Corporations",
                        principalColumn: "CorporationId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CxCContractorDetails",
                columns: table => new
                {
                    CxCContractorDetailId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CxCContractorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DatePayment = table.Column<DateTime>(type: "date", nullable: false),
                    PaymentMode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Reference = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Detail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Debt = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Payment = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Balance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CorporationId = table.Column<int>(type: "int", nullable: false),
                    UsuarioOwner = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CxCContractorDetails", x => x.CxCContractorDetailId);
                    table.ForeignKey(
                        name: "FK_CxCContractorDetails_Corporations_CorporationId",
                        column: x => x.CorporationId,
                        principalTable: "Corporations",
                        principalColumn: "CorporationId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CxCContractorDetails_CxCContractors_CxCContractorId",
                        column: x => x.CxCContractorId,
                        principalTable: "CxCContractors",
                        principalColumn: "CxCContractorId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContractorAccountPayables_CorporationId_ContractorId_CxCContractorId",
                table: "ContractorAccountPayables",
                columns: new[] { "CorporationId", "ContractorId", "CxCContractorId" });

            migrationBuilder.CreateIndex(
                name: "IX_ContractorAccountPayables_CxCContractorId",
                table: "ContractorAccountPayables",
                column: "CxCContractorId");

            migrationBuilder.CreateIndex(
                name: "IX_CxCContractorDetails_CorporationId",
                table: "CxCContractorDetails",
                column: "CorporationId");

            migrationBuilder.CreateIndex(
                name: "IX_CxCContractorDetails_CxCContractorId",
                table: "CxCContractorDetails",
                column: "CxCContractorId");

            migrationBuilder.CreateIndex(
                name: "IX_CxCContractors_ContractorId",
                table: "CxCContractors",
                column: "ContractorId");

            migrationBuilder.CreateIndex(
                name: "IX_CxCContractors_CorporationId_DateNote",
                table: "CxCContractors",
                columns: new[] { "CorporationId", "DateNote" });

            migrationBuilder.CreateIndex(
                name: "IX_CxCContractors_CorporationId_NoteNumber",
                table: "CxCContractors",
                columns: new[] { "CorporationId", "NoteNumber" },
                unique: true,
                filter: "[NoteNumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CxCContractors_CorporationId_Paid_Cancelled",
                table: "CxCContractors",
                columns: new[] { "CorporationId", "Paid", "Cancelled" });

            migrationBuilder.AddForeignKey(
                name: "FK_ContractorAccountPayables_CxCContractors_CxCContractorId",
                table: "ContractorAccountPayables",
                column: "CxCContractorId",
                principalTable: "CxCContractors",
                principalColumn: "CxCContractorId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContractorAccountPayables_CxCContractors_CxCContractorId",
                table: "ContractorAccountPayables");

            migrationBuilder.DropTable(
                name: "CxCContractorDetails");

            migrationBuilder.DropTable(
                name: "CxCContractors");

            migrationBuilder.DropIndex(
                name: "IX_ContractorAccountPayables_CorporationId_ContractorId_CxCContractorId",
                table: "ContractorAccountPayables");

            migrationBuilder.DropIndex(
                name: "IX_ContractorAccountPayables_CxCContractorId",
                table: "ContractorAccountPayables");

            migrationBuilder.DropColumn(
                name: "CxCContractorId",
                table: "ContractorAccountPayables");
        }
    }
}
