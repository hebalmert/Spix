using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spix.AppBacken.Migrations
{
    /// <inheritdoc />
    public partial class UpdateServiceGeoHelper : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TechnicianComment",
                table: "ServiceRequests",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CapturedAtUtc",
                table: "ServiceRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DistanceMeters",
                table: "ServiceRequests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Latitude",
                table: "ServiceRequests",
                type: "decimal(12,7)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Longitude",
                table: "ServiceRequests",
                type: "decimal(12,7)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ServiceRequestParentId",
                table: "ServiceRequests",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CapturedAtUtc",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "DistanceMeters",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "ServiceRequestParentId",
                table: "ServiceRequests");

            migrationBuilder.AlterColumn<string>(
                name: "TechnicianComment",
                table: "ServiceRequests",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
