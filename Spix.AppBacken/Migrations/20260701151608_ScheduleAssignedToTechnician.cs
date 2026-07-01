using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spix.AppBacken.Migrations
{
    /// <inheritdoc />
    public partial class ScheduleAssignedToTechnician : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM [ScheduleItems]");

            migrationBuilder.DropForeignKey(
                name: "FK_ScheduleItems_Corporations_CorporationId",
                table: "ScheduleItems");

            migrationBuilder.DropForeignKey(
                name: "FK_ScheduleItems_Usuarios_UsuarioId",
                table: "ScheduleItems");

            migrationBuilder.RenameColumn(
                name: "UsuarioId",
                table: "ScheduleItems",
                newName: "TechnicianId");

            migrationBuilder.RenameIndex(
                name: "IX_ScheduleItems_UsuarioId",
                table: "ScheduleItems",
                newName: "IX_ScheduleItems_TechnicianId");

            migrationBuilder.AlterColumn<Guid>(
                name: "ScheduleItemId",
                table: "ScheduleItems",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWSEQUENTIALID()",
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduleItems_Corporations_CorporationId",
                table: "ScheduleItems",
                column: "CorporationId",
                principalTable: "Corporations",
                principalColumn: "CorporationId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduleItems_Technicians_TechnicianId",
                table: "ScheduleItems",
                column: "TechnicianId",
                principalTable: "Technicians",
                principalColumn: "TechnicianId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScheduleItems_Corporations_CorporationId",
                table: "ScheduleItems");

            migrationBuilder.DropForeignKey(
                name: "FK_ScheduleItems_Technicians_TechnicianId",
                table: "ScheduleItems");

            migrationBuilder.RenameColumn(
                name: "TechnicianId",
                table: "ScheduleItems",
                newName: "UsuarioId");

            migrationBuilder.RenameIndex(
                name: "IX_ScheduleItems_TechnicianId",
                table: "ScheduleItems",
                newName: "IX_ScheduleItems_UsuarioId");

            migrationBuilder.AlterColumn<Guid>(
                name: "ScheduleItemId",
                table: "ScheduleItems",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldDefaultValueSql: "NEWSEQUENTIALID()");

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduleItems_Corporations_CorporationId",
                table: "ScheduleItems",
                column: "CorporationId",
                principalTable: "Corporations",
                principalColumn: "CorporationId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduleItems_Usuarios_UsuarioId",
                table: "ScheduleItems",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "UsuarioId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
