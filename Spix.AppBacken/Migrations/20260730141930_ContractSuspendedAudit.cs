using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spix.AppBacken.Migrations
{
    /// <inheritdoc />
    public partial class ContractSuspendedAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContractSuspendedAudits",
                columns: table => new
                {
                    ContractSuspendedAuditId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    ContractId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DateModified = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserByName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CorporationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractSuspendedAudits", x => x.ContractSuspendedAuditId);
                    table.ForeignKey(
                        name: "FK_ContractSuspendedAudits_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "ClientId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContractSuspendedAudits_ContractClients_ContractId",
                        column: x => x.ContractId,
                        principalTable: "ContractClients",
                        principalColumn: "ContractClientId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContractSuspendedAudits_Corporations_CorporationId",
                        column: x => x.CorporationId,
                        principalTable: "Corporations",
                        principalColumn: "CorporationId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContractSuspendedAudits_ClientId",
                table: "ContractSuspendedAudits",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractSuspendedAudits_ContractId",
                table: "ContractSuspendedAudits",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractSuspendedAudits_CorporationId_DateModified",
                table: "ContractSuspendedAudits",
                columns: new[] { "CorporationId", "DateModified" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContractSuspendedAudits");
        }
    }
}
