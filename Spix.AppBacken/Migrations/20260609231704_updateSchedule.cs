using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spix.AppBacken.Migrations
{
    /// <inheritdoc />
    public partial class updateSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScheduleItems_AspNetUsers_CreatedByUserId",
                table: "ScheduleItems");

            migrationBuilder.DropIndex(
                name: "IX_ScheduleItems_CreatedByUserId",
                table: "ScheduleItems");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "ScheduleItems");

            migrationBuilder.AddColumn<int>(
                name: "CorporationId",
                table: "ScheduleItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "ScheduleItems",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UsuarioOwner",
                table: "ScheduleItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleItems_CorporationId",
                table: "ScheduleItems",
                column: "CorporationId");

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduleItems_Corporations_CorporationId",
                table: "ScheduleItems",
                column: "CorporationId",
                principalTable: "Corporations",
                principalColumn: "CorporationId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScheduleItems_Corporations_CorporationId",
                table: "ScheduleItems");

            migrationBuilder.DropIndex(
                name: "IX_ScheduleItems_CorporationId",
                table: "ScheduleItems");

            migrationBuilder.DropColumn(
                name: "CorporationId",
                table: "ScheduleItems");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "ScheduleItems");

            migrationBuilder.DropColumn(
                name: "UsuarioOwner",
                table: "ScheduleItems");

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "ScheduleItems",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleItems_CreatedByUserId",
                table: "ScheduleItems",
                column: "CreatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduleItems_AspNetUsers_CreatedByUserId",
                table: "ScheduleItems",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
