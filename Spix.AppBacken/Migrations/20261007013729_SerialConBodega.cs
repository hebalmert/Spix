using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spix.AppBacken.Migrations
{
    /// <inheritdoc />
    public partial class SerialConBodega : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CargueDetails_CorporationId",
                table: "CargueDetails");

            migrationBuilder.AddColumn<Guid>(
                name: "ProductStorageId",
                table: "CargueDetails",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TransferDetailsId",
                table: "CargueDetails",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CargueDetails_CorporationId_ProductStorageId_Status",
                table: "CargueDetails",
                columns: new[] { "CorporationId", "ProductStorageId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_CargueDetails_ProductStorageId",
                table: "CargueDetails",
                column: "ProductStorageId");

            migrationBuilder.CreateIndex(
                name: "IX_CargueDetails_TransferDetailsId",
                table: "CargueDetails",
                column: "TransferDetailsId");

            migrationBuilder.AddForeignKey(
                name: "FK_CargueDetails_ProductStorages_ProductStorageId",
                table: "CargueDetails",
                column: "ProductStorageId",
                principalTable: "ProductStorages",
                principalColumn: "ProductStorageId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CargueDetails_TransferDetails_TransferDetailsId",
                table: "CargueDetails",
                column: "TransferDetailsId",
                principalTable: "TransferDetails",
                principalColumn: "TransferDetailsId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CargueDetails_ProductStorages_ProductStorageId",
                table: "CargueDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_CargueDetails_TransferDetails_TransferDetailsId",
                table: "CargueDetails");

            migrationBuilder.DropIndex(
                name: "IX_CargueDetails_CorporationId_ProductStorageId_Status",
                table: "CargueDetails");

            migrationBuilder.DropIndex(
                name: "IX_CargueDetails_ProductStorageId",
                table: "CargueDetails");

            migrationBuilder.DropIndex(
                name: "IX_CargueDetails_TransferDetailsId",
                table: "CargueDetails");

            migrationBuilder.DropColumn(
                name: "ProductStorageId",
                table: "CargueDetails");

            migrationBuilder.DropColumn(
                name: "TransferDetailsId",
                table: "CargueDetails");

            migrationBuilder.CreateIndex(
                name: "IX_CargueDetails_CorporationId",
                table: "CargueDetails",
                column: "CorporationId");
        }
    }
}
