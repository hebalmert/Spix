using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spix.AppBacken.Migrations
{
    /// <inheritdoc />
    public partial class TrasladoSerialesMovidos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TransferDetailSerials",
                columns: table => new
                {
                    TransferDetailSerialId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    TransferDetailsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CargueDetailId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MacWlan = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DateMoved = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CorporationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransferDetailSerials", x => x.TransferDetailSerialId);
                    table.ForeignKey(
                        name: "FK_TransferDetailSerials_CargueDetails_CargueDetailId",
                        column: x => x.CargueDetailId,
                        principalTable: "CargueDetails",
                        principalColumn: "CargueDetailId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransferDetailSerials_Corporations_CorporationId",
                        column: x => x.CorporationId,
                        principalTable: "Corporations",
                        principalColumn: "CorporationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TransferDetailSerials_TransferDetails_TransferDetailsId",
                        column: x => x.TransferDetailsId,
                        principalTable: "TransferDetails",
                        principalColumn: "TransferDetailsId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TransferDetailSerials_CargueDetailId",
                table: "TransferDetailSerials",
                column: "CargueDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferDetailSerials_CorporationId_TransferDetailsId",
                table: "TransferDetailSerials",
                columns: new[] { "CorporationId", "TransferDetailsId" });

            migrationBuilder.CreateIndex(
                name: "IX_TransferDetailSerials_TransferDetailsId_CargueDetailId",
                table: "TransferDetailSerials",
                columns: new[] { "TransferDetailsId", "CargueDetailId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TransferDetailSerials");
        }
    }
}
