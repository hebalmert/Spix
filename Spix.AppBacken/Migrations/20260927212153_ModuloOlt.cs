using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spix.AppBacken.Migrations
{
    /// <inheritdoc />
    public partial class ModuloOlt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Olts",
                columns: table => new
                {
                    OltId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    OltName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IpNetworkId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Usuario = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    Clave = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    MarkId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MarkModelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ZoneId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Latitude = table.Column<decimal>(type: "decimal(12,7)", nullable: true),
                    Longitude = table.Column<decimal>(type: "decimal(12,7)", nullable: true),
                    PortCount = table.Column<int>(type: "int", nullable: false),
                    PortSpeed = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    CorporationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Olts", x => x.OltId);
                    table.ForeignKey(
                        name: "FK_Olts_Corporations_CorporationId",
                        column: x => x.CorporationId,
                        principalTable: "Corporations",
                        principalColumn: "CorporationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Olts_IpNetworks_IpNetworkId",
                        column: x => x.IpNetworkId,
                        principalTable: "IpNetworks",
                        principalColumn: "IpNetworkId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Olts_MarkModels_MarkModelId",
                        column: x => x.MarkModelId,
                        principalTable: "MarkModels",
                        principalColumn: "MarkModelId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Olts_Marks_MarkId",
                        column: x => x.MarkId,
                        principalTable: "Marks",
                        principalColumn: "MarkId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Olts_Zones_ZoneId",
                        column: x => x.ZoneId,
                        principalTable: "Zones",
                        principalColumn: "ZoneId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ContractOlts",
                columns: table => new
                {
                    ContractOltId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    ContractClientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OltId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractOlts", x => x.ContractOltId);
                    table.ForeignKey(
                        name: "FK_ContractOlts_ContractClients_ContractClientId",
                        column: x => x.ContractClientId,
                        principalTable: "ContractClients",
                        principalColumn: "ContractClientId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContractOlts_Olts_OltId",
                        column: x => x.OltId,
                        principalTable: "Olts",
                        principalColumn: "OltId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContractOlts_ContractClientId",
                table: "ContractOlts",
                column: "ContractClientId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContractOlts_OltId",
                table: "ContractOlts",
                column: "OltId");

            migrationBuilder.CreateIndex(
                name: "IX_Olts_CorporationId",
                table: "Olts",
                column: "CorporationId");

            migrationBuilder.CreateIndex(
                name: "IX_Olts_IpNetworkId",
                table: "Olts",
                column: "IpNetworkId");

            migrationBuilder.CreateIndex(
                name: "IX_Olts_MarkId",
                table: "Olts",
                column: "MarkId");

            migrationBuilder.CreateIndex(
                name: "IX_Olts_MarkModelId",
                table: "Olts",
                column: "MarkModelId");

            migrationBuilder.CreateIndex(
                name: "IX_Olts_OltName_CorporationId",
                table: "Olts",
                columns: new[] { "OltName", "CorporationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Olts_ZoneId",
                table: "Olts",
                column: "ZoneId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContractOlts");

            migrationBuilder.DropTable(
                name: "Olts");
        }
    }
}
