using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spix.AppBacken.Migrations
{
    /// <inheritdoc />
    public partial class ProductMarkAndModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "MarkId",
                table: "Products",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MarkModelId",
                table: "Products",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_MarkId",
                table: "Products",
                column: "MarkId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_MarkModelId",
                table: "Products",
                column: "MarkModelId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_MarkModels_MarkModelId",
                table: "Products",
                column: "MarkModelId",
                principalTable: "MarkModels",
                principalColumn: "MarkModelId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Marks_MarkId",
                table: "Products",
                column: "MarkId",
                principalTable: "Marks",
                principalColumn: "MarkId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_MarkModels_MarkModelId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Marks_MarkId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_MarkId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_MarkModelId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "MarkId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "MarkModelId",
                table: "Products");
        }
    }
}
