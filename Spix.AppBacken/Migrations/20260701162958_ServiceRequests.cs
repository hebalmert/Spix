using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spix.AppBacken.Migrations
{
    /// <inheritdoc />
    public partial class ServiceRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Origin",
                table: "ScheduleItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "ServiceRequestId",
                table: "ScheduleItems",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ServiceRequests",
                columns: table => new
                {
                    ServiceRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    RequestNumber = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ScheduledAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ContractClientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TechnicianId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ScheduleStatus = table.Column<int>(type: "int", nullable: false),
                    ClientReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TechnicianComment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Recommendation = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ControlContrato = table.Column<long>(type: "bigint", nullable: false),
                    ClientFullName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    CityName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ZoneName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ServerName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IpServer = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IpCliente = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MacCliente = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PlanName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PlanSpeed = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    CorporationId = table.Column<int>(type: "int", nullable: false),
                    UsuarioOwner = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceRequests", x => x.ServiceRequestId);
                    table.ForeignKey(
                        name: "FK_ServiceRequests_ContractClients_ContractClientId",
                        column: x => x.ContractClientId,
                        principalTable: "ContractClients",
                        principalColumn: "ContractClientId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServiceRequests_Corporations_CorporationId",
                        column: x => x.CorporationId,
                        principalTable: "Corporations",
                        principalColumn: "CorporationId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServiceRequests_Technicians_TechnicianId",
                        column: x => x.TechnicianId,
                        principalTable: "Technicians",
                        principalColumn: "TechnicianId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ServiceRequestDetails",
                columns: table => new
                {
                    ServiceRequestDetailId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    ServiceRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ServiceCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ServiceClientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Detail = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceRequestDetails", x => x.ServiceRequestDetailId);
                    table.ForeignKey(
                        name: "FK_ServiceRequestDetails_ServiceCategories_ServiceCategoryId",
                        column: x => x.ServiceCategoryId,
                        principalTable: "ServiceCategories",
                        principalColumn: "ServiceCategoryId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServiceRequestDetails_ServiceClients_ServiceClientId",
                        column: x => x.ServiceClientId,
                        principalTable: "ServiceClients",
                        principalColumn: "ServiceClientId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServiceRequestDetails_ServiceRequests_ServiceRequestId",
                        column: x => x.ServiceRequestId,
                        principalTable: "ServiceRequests",
                        principalColumn: "ServiceRequestId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleItems_ServiceRequestId",
                table: "ScheduleItems",
                column: "ServiceRequestId",
                unique: true,
                filter: "[ServiceRequestId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequestDetails_ServiceCategoryId",
                table: "ServiceRequestDetails",
                column: "ServiceCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequestDetails_ServiceClientId",
                table: "ServiceRequestDetails",
                column: "ServiceClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequestDetails_ServiceRequestId",
                table: "ServiceRequestDetails",
                column: "ServiceRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_ContractClientId",
                table: "ServiceRequests",
                column: "ContractClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_CorporationId_RequestNumber",
                table: "ServiceRequests",
                columns: new[] { "CorporationId", "RequestNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_TechnicianId",
                table: "ServiceRequests",
                column: "TechnicianId");

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduleItems_ServiceRequests_ServiceRequestId",
                table: "ScheduleItems",
                column: "ServiceRequestId",
                principalTable: "ServiceRequests",
                principalColumn: "ServiceRequestId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScheduleItems_ServiceRequests_ServiceRequestId",
                table: "ScheduleItems");

            migrationBuilder.DropTable(
                name: "ServiceRequestDetails");

            migrationBuilder.DropTable(
                name: "ServiceRequests");

            migrationBuilder.DropIndex(
                name: "IX_ScheduleItems_ServiceRequestId",
                table: "ScheduleItems");

            migrationBuilder.DropColumn(
                name: "Origin",
                table: "ScheduleItems");

            migrationBuilder.DropColumn(
                name: "ServiceRequestId",
                table: "ScheduleItems");
        }
    }
}
