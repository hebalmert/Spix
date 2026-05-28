using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spix.AppBacken.Migrations
{
    /// <inheritdoc />
    public partial class NewContractBind : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContractBinds",
                columns: table => new
                {
                    ContractBindId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    ContractClientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ServerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IpNetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CargueDetailId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HotSpotTypeId = table.Column<int>(type: "int", nullable: false),
                    MikrotikId = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    ServerName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IpServer = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IpCliente = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MacCliente = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractBinds", x => x.ContractBindId);
                    table.ForeignKey(
                        name: "FK_ContractBinds_CargueDetails_CargueDetailId",
                        column: x => x.CargueDetailId,
                        principalTable: "CargueDetails",
                        principalColumn: "CargueDetailId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContractBinds_ContractClients_ContractClientId",
                        column: x => x.ContractClientId,
                        principalTable: "ContractClients",
                        principalColumn: "ContractClientId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContractBinds_HotSpotTypes_HotSpotTypeId",
                        column: x => x.HotSpotTypeId,
                        principalTable: "HotSpotTypes",
                        principalColumn: "HotSpotTypeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContractBinds_IpNets_IpNetId",
                        column: x => x.IpNetId,
                        principalTable: "IpNets",
                        principalColumn: "IpNetId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContractBinds_Servers_ServerId",
                        column: x => x.ServerId,
                        principalTable: "Servers",
                        principalColumn: "ServerId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContractBinds_CargueDetailId",
                table: "ContractBinds",
                column: "CargueDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractBinds_ContractClientId_IpNetId",
                table: "ContractBinds",
                columns: new[] { "ContractClientId", "IpNetId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContractBinds_HotSpotTypeId",
                table: "ContractBinds",
                column: "HotSpotTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractBinds_IpNetId",
                table: "ContractBinds",
                column: "IpNetId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractBinds_ServerId",
                table: "ContractBinds",
                column: "ServerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContractBinds");
        }
    }
}
