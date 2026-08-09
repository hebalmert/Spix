using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spix.AppBacken.Migrations
{
    /// <inheritdoc />
    public partial class AddEstratoSocial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "EstratoSocialId",
                table: "ContractClients",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EstratosSociales",
                columns: table => new
                {
                    EstratoSocialId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    EstratoSocialName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ApplyTax = table.Column<bool>(type: "bit", nullable: false),
                    CorporationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstratosSociales", x => x.EstratoSocialId);
                    table.ForeignKey(
                        name: "FK_EstratosSociales_Corporations_CorporationId",
                        column: x => x.CorporationId,
                        principalTable: "Corporations",
                        principalColumn: "CorporationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContractClients_EstratoSocialId",
                table: "ContractClients",
                column: "EstratoSocialId");

            migrationBuilder.CreateIndex(
                name: "IX_EstratosSociales_CorporationId_EstratoSocialName",
                table: "EstratosSociales",
                columns: new[] { "CorporationId", "EstratoSocialName" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ContractClients_EstratosSociales_EstratoSocialId",
                table: "ContractClients",
                column: "EstratoSocialId",
                principalTable: "EstratosSociales",
                principalColumn: "EstratoSocialId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContractClients_EstratosSociales_EstratoSocialId",
                table: "ContractClients");

            migrationBuilder.DropTable(
                name: "EstratosSociales");

            migrationBuilder.DropIndex(
                name: "IX_ContractClients_EstratoSocialId",
                table: "ContractClients");

            migrationBuilder.DropColumn(
                name: "EstratoSocialId",
                table: "ContractClients");
        }
    }
}
