using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spix.AppBacken.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCxCBillCancelletion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DescriptionCancelled",
                table: "CxCBills",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserIdCancelled",
                table: "CxCBills",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UsuarioOwnerCancelled",
                table: "CxCBills",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DescriptionCancelled",
                table: "CxCBills");

            migrationBuilder.DropColumn(
                name: "UserIdCancelled",
                table: "CxCBills");

            migrationBuilder.DropColumn(
                name: "UsuarioOwnerCancelled",
                table: "CxCBills");
        }
    }
}
