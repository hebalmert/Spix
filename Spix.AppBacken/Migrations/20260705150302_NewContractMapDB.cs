using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spix.AppBacken.Migrations
{
    /// <inheritdoc />
    public partial class NewContractMapDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContractMaps",
                columns: table => new
                {
                    ContractMapId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    ContractClientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Latitude = table.Column<decimal>(type: "decimal(12,7)", precision: 12, scale: 7, nullable: true),
                    Longitude = table.Column<decimal>(type: "decimal(12,7)", precision: 12, scale: 7, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractMaps", x => x.ContractMapId);
                    table.ForeignKey(
                        name: "FK_ContractMaps_ContractClients_ContractClientId",
                        column: x => x.ContractClientId,
                        principalTable: "ContractClients",
                        principalColumn: "ContractClientId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContractMaps_ContractClientId",
                table: "ContractMaps",
                column: "ContractClientId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContractMaps");
        }
    }
}
