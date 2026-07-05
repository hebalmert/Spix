using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spix.AppBacken.Migrations
{
    /// <inheritdoc />
    public partial class NewServiceRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ServiceRequestPics",
                columns: table => new
                {
                    ServiceRequestPicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    ServiceRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PhotoBefore1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhotoBefore2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhotoAfter1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhotoAfter2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UsuarioOwner = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CorporationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceRequestPics", x => x.ServiceRequestPicId);
                    table.ForeignKey(
                        name: "FK_ServiceRequestPics_Corporations_CorporationId",
                        column: x => x.CorporationId,
                        principalTable: "Corporations",
                        principalColumn: "CorporationId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServiceRequestPics_ServiceRequests_ServiceRequestId",
                        column: x => x.ServiceRequestId,
                        principalTable: "ServiceRequests",
                        principalColumn: "ServiceRequestId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequestPics_CorporationId",
                table: "ServiceRequestPics",
                column: "CorporationId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequestPics_ServiceRequestId",
                table: "ServiceRequestPics",
                column: "ServiceRequestId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServiceRequestPics");
        }
    }
}
