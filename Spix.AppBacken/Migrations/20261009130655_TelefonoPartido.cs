using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spix.AppBacken.Migrations
{
    /// <inheritdoc />
    public partial class TelefonoPartido : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CodeCountry",
                table: "Usuarios",
                type: "nvarchar(6)",
                maxLength: 6,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CodeNumber",
                table: "Usuarios",
                type: "nvarchar(6)",
                maxLength: 6,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CodeCountry",
                table: "Technicians",
                type: "nvarchar(6)",
                maxLength: 6,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CodeNumber",
                table: "Technicians",
                type: "nvarchar(6)",
                maxLength: 6,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CodeCountry",
                table: "Contractors",
                type: "nvarchar(6)",
                maxLength: 6,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CodeNumber",
                table: "Contractors",
                type: "nvarchar(6)",
                maxLength: 6,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CodeCountry",
                table: "ContractClients",
                type: "nvarchar(6)",
                maxLength: 6,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CodeCountry2",
                table: "ContractClients",
                type: "nvarchar(6)",
                maxLength: 6,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CodeNumber",
                table: "ContractClients",
                type: "nvarchar(6)",
                maxLength: 6,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CodeNumber2",
                table: "ContractClients",
                type: "nvarchar(6)",
                maxLength: 6,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CodeCountry",
                table: "Clients",
                type: "nvarchar(6)",
                maxLength: 6,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CodeNumber",
                table: "Clients",
                type: "nvarchar(6)",
                maxLength: 6,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CodeCountry",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "CodeNumber",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "CodeCountry",
                table: "Technicians");

            migrationBuilder.DropColumn(
                name: "CodeNumber",
                table: "Technicians");

            migrationBuilder.DropColumn(
                name: "CodeCountry",
                table: "Contractors");

            migrationBuilder.DropColumn(
                name: "CodeNumber",
                table: "Contractors");

            migrationBuilder.DropColumn(
                name: "CodeCountry",
                table: "ContractClients");

            migrationBuilder.DropColumn(
                name: "CodeCountry2",
                table: "ContractClients");

            migrationBuilder.DropColumn(
                name: "CodeNumber",
                table: "ContractClients");

            migrationBuilder.DropColumn(
                name: "CodeNumber2",
                table: "ContractClients");

            migrationBuilder.DropColumn(
                name: "CodeCountry",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "CodeNumber",
                table: "Clients");
        }
    }
}
