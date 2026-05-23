using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spix.AppBacken.Migrations
{
    /// <inheritdoc />
    public partial class UpdateContractClientsDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContractClients_ServiceCategories_ServiceCategoryId",
                table: "ContractClients");

            migrationBuilder.DropForeignKey(
                name: "FK_ContractClients_ServiceClients_ServiceClientId",
                table: "ContractClients");

            migrationBuilder.DropIndex(
                name: "IX_ContractClients_CorporationId_ControlContrato",
                table: "ContractClients");

            migrationBuilder.DropColumn(
                name: "CodeCountry",
                table: "ContractClients");

            migrationBuilder.DropColumn(
                name: "CodeNumber",
                table: "ContractClients");

            migrationBuilder.DropColumn(
                name: "Impuesto",
                table: "ContractClients");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "ContractClients");

            migrationBuilder.DropColumn(
                name: "ServiceName",
                table: "ContractClients");

            migrationBuilder.RenameColumn(
                name: "StateType",
                table: "ContractClients",
                newName: "ContractState");

            migrationBuilder.AlterColumn<string>(
                name: "Document",
                table: "Technicians",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(25)",
                oldMaxLength: 25);

            migrationBuilder.AlterColumn<Guid>(
                name: "ServiceClientId",
                table: "ContractClients",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "ServiceCategoryId",
                table: "ContractClients",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<long>(
                name: "ControlContrato",
                table: "ContractClients",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "ContractClients",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UsuarioOwner",
                table: "ContractClients",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContractClients_CorporationId_ControlContrato",
                table: "ContractClients",
                columns: new[] { "CorporationId", "ControlContrato" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ContractClients_ServiceCategories_ServiceCategoryId",
                table: "ContractClients",
                column: "ServiceCategoryId",
                principalTable: "ServiceCategories",
                principalColumn: "ServiceCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_ContractClients_ServiceClients_ServiceClientId",
                table: "ContractClients",
                column: "ServiceClientId",
                principalTable: "ServiceClients",
                principalColumn: "ServiceClientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContractClients_ServiceCategories_ServiceCategoryId",
                table: "ContractClients");

            migrationBuilder.DropForeignKey(
                name: "FK_ContractClients_ServiceClients_ServiceClientId",
                table: "ContractClients");

            migrationBuilder.DropIndex(
                name: "IX_ContractClients_CorporationId_ControlContrato",
                table: "ContractClients");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "ContractClients");

            migrationBuilder.DropColumn(
                name: "UsuarioOwner",
                table: "ContractClients");

            migrationBuilder.RenameColumn(
                name: "ContractState",
                table: "ContractClients",
                newName: "StateType");

            migrationBuilder.AlterColumn<string>(
                name: "Document",
                table: "Technicians",
                type: "nvarchar(25)",
                maxLength: 25,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(15)",
                oldMaxLength: 15);

            migrationBuilder.AlterColumn<Guid>(
                name: "ServiceClientId",
                table: "ContractClients",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "ServiceCategoryId",
                table: "ContractClients",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ControlContrato",
                table: "ContractClients",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<string>(
                name: "CodeCountry",
                table: "ContractClients",
                type: "nvarchar(7)",
                maxLength: 7,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CodeNumber",
                table: "ContractClients",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "Impuesto",
                table: "ContractClients",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "ContractClients",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ServiceName",
                table: "ContractClients",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContractClients_CorporationId_ControlContrato",
                table: "ContractClients",
                columns: new[] { "CorporationId", "ControlContrato" },
                unique: true,
                filter: "[ControlContrato] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_ContractClients_ServiceCategories_ServiceCategoryId",
                table: "ContractClients",
                column: "ServiceCategoryId",
                principalTable: "ServiceCategories",
                principalColumn: "ServiceCategoryId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ContractClients_ServiceClients_ServiceClientId",
                table: "ContractClients",
                column: "ServiceClientId",
                principalTable: "ServiceClients",
                principalColumn: "ServiceClientId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
