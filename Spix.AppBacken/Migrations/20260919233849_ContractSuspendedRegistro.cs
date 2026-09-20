using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spix.AppBacken.Migrations
{
    /// <inheritdoc />
    public partial class ContractSuspendedRegistro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContractSuspendeds",
                columns: table => new
                {
                    ContractSuspendedId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    ContractClientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DateSuspended = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateReactivated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Origin = table.Column<int>(type: "int", nullable: false),
                    RunSuspendedId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Motivo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ControlContrato = table.Column<long>(type: "bigint", nullable: false),
                    ClientName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ClientDocument = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ContractAddress = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ContractPhone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CityName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ZoneName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PlanName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PlanAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserByName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    UserIdReactivated = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserByNameReactivated = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    CorporationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractSuspendeds", x => x.ContractSuspendedId);
                    table.ForeignKey(
                        name: "FK_ContractSuspendeds_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "ClientId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContractSuspendeds_ContractClients_ContractClientId",
                        column: x => x.ContractClientId,
                        principalTable: "ContractClients",
                        principalColumn: "ContractClientId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContractSuspendeds_Corporations_CorporationId",
                        column: x => x.CorporationId,
                        principalTable: "Corporations",
                        principalColumn: "CorporationId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContractSuspendeds_RunSuspendeds_RunSuspendedId",
                        column: x => x.RunSuspendedId,
                        principalTable: "RunSuspendeds",
                        principalColumn: "RunSuspendedId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContractSuspendeds_ClientId",
                table: "ContractSuspendeds",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractSuspendeds_ContractClientId_DateReactivated",
                table: "ContractSuspendeds",
                columns: new[] { "ContractClientId", "DateReactivated" });

            migrationBuilder.CreateIndex(
                name: "IX_ContractSuspendeds_CorporationId_DateSuspended",
                table: "ContractSuspendeds",
                columns: new[] { "CorporationId", "DateSuspended" });

            migrationBuilder.CreateIndex(
                name: "IX_ContractSuspendeds_RunSuspendedId",
                table: "ContractSuspendeds",
                column: "RunSuspendedId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContractSuspendeds");
        }
    }
}
