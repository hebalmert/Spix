using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spix.AppBacken.Migrations
{
    /// <inheritdoc />
    public partial class IpLocalPppoeDesdeRed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Servers_IpNets_PppLocalIpNetId",
                table: "Servers");

            migrationBuilder.AddForeignKey(
                name: "FK_Servers_IpNetworks_PppLocalIpNetId",
                table: "Servers",
                column: "PppLocalIpNetId",
                principalTable: "IpNetworks",
                principalColumn: "IpNetworkId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Servers_IpNetworks_PppLocalIpNetId",
                table: "Servers");

            migrationBuilder.AddForeignKey(
                name: "FK_Servers_IpNets_PppLocalIpNetId",
                table: "Servers",
                column: "PppLocalIpNetId",
                principalTable: "IpNets",
                principalColumn: "IpNetId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
