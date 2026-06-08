using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spix.AppBacken.Migrations
{
    /// <inheritdoc />
    public partial class AddRefreshTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContractMacs",
                columns: table => new
                {
                    ContractMacId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    ContractClientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CargueDetailId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractMacs", x => x.ContractMacId);
                    table.ForeignKey(
                        name: "FK_ContractMacs_CargueDetails_CargueDetailId",
                        column: x => x.CargueDetailId,
                        principalTable: "CargueDetails",
                        principalColumn: "CargueDetailId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContractMacs_ContractClients_ContractClientId",
                        column: x => x.ContractClientId,
                        principalTable: "ContractClients",
                        principalColumn: "ContractClientId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Expiration = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRevoked = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContractMacs_CargueDetailId",
                table: "ContractMacs",
                column: "CargueDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractMacs_ContractClientId_CargueDetailId",
                table: "ContractMacs",
                columns: new[] { "ContractClientId", "CargueDetailId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContractMacs");

            migrationBuilder.DropTable(
                name: "RefreshTokens");
        }
    }
}
