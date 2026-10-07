using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spix.AppBacken.Migrations
{
    /// <inheritdoc />
    public partial class TrasladoAuditoria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateClosed",
                table: "Transfers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateCreated",
                table: "Transfers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NombreUsuarioCierre",
                table: "Transfers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReceivedByName",
                table: "Transfers",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReceivedByTechnicianId",
                table: "Transfers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReceivedByUsuarioId",
                table: "Transfers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserIdClosed",
                table: "Transfers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_ReceivedByTechnicianId",
                table: "Transfers",
                column: "ReceivedByTechnicianId");

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_ReceivedByUsuarioId",
                table: "Transfers",
                column: "ReceivedByUsuarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transfers_Technicians_ReceivedByTechnicianId",
                table: "Transfers",
                column: "ReceivedByTechnicianId",
                principalTable: "Technicians",
                principalColumn: "TechnicianId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Transfers_Usuarios_ReceivedByUsuarioId",
                table: "Transfers",
                column: "ReceivedByUsuarioId",
                principalTable: "Usuarios",
                principalColumn: "UsuarioId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transfers_Technicians_ReceivedByTechnicianId",
                table: "Transfers");

            migrationBuilder.DropForeignKey(
                name: "FK_Transfers_Usuarios_ReceivedByUsuarioId",
                table: "Transfers");

            migrationBuilder.DropIndex(
                name: "IX_Transfers_ReceivedByTechnicianId",
                table: "Transfers");

            migrationBuilder.DropIndex(
                name: "IX_Transfers_ReceivedByUsuarioId",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "DateClosed",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "DateCreated",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "NombreUsuarioCierre",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "ReceivedByName",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "ReceivedByTechnicianId",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "ReceivedByUsuarioId",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "UserIdClosed",
                table: "Transfers");
        }
    }
}
