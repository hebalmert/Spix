using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spix.AppBacken.Migrations
{
    /// <inheritdoc />
    public partial class AddRunSuspended : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RunSuspendeds",
                columns: table => new
                {
                    RunSuspendedId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    YearNumber = table.Column<int>(type: "int", nullable: false),
                    MonthType = table.Column<int>(type: "int", nullable: false),
                    Executed = table.Column<bool>(type: "bit", nullable: false),
                    DateUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserByName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    CorporationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RunSuspendeds", x => x.RunSuspendedId);
                    table.ForeignKey(
                        name: "FK_RunSuspendeds_Corporations_CorporationId",
                        column: x => x.CorporationId,
                        principalTable: "Corporations",
                        principalColumn: "CorporationId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RunSuspendedDetails",
                columns: table => new
                {
                    RunSuspendedDetailId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    RunSuspendedId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContractClientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CxCBillId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DateUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PlanAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RunSuspendedDetails", x => x.RunSuspendedDetailId);
                    table.ForeignKey(
                        name: "FK_RunSuspendedDetails_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "ClientId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RunSuspendedDetails_ContractClients_ContractClientId",
                        column: x => x.ContractClientId,
                        principalTable: "ContractClients",
                        principalColumn: "ContractClientId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RunSuspendedDetails_CxCBills_CxCBillId",
                        column: x => x.CxCBillId,
                        principalTable: "CxCBills",
                        principalColumn: "CxCBillId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RunSuspendedDetails_RunSuspendeds_RunSuspendedId",
                        column: x => x.RunSuspendedId,
                        principalTable: "RunSuspendeds",
                        principalColumn: "RunSuspendedId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RunSuspendedDetails_ClientId",
                table: "RunSuspendedDetails",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_RunSuspendedDetails_ContractClientId",
                table: "RunSuspendedDetails",
                column: "ContractClientId");

            migrationBuilder.CreateIndex(
                name: "IX_RunSuspendedDetails_CxCBillId",
                table: "RunSuspendedDetails",
                column: "CxCBillId");

            migrationBuilder.CreateIndex(
                name: "IX_RunSuspendedDetails_RunSuspendedId_ContractClientId",
                table: "RunSuspendedDetails",
                columns: new[] { "RunSuspendedId", "ContractClientId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RunSuspendeds_CorporationId_YearNumber_MonthType",
                table: "RunSuspendeds",
                columns: new[] { "CorporationId", "YearNumber", "MonthType" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RunSuspendedDetails");

            migrationBuilder.DropTable(
                name: "RunSuspendeds");
        }
    }
}
