using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spix.AppBacken.Migrations
{
    /// <inheritdoc />
    public partial class ContractExemptExoneradoYBitacora : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PreExonerateds");

            migrationBuilder.CreateTable(
                name: "ContractAudits",
                columns: table => new
                {
                    ContractAuditId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    ContractClientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DateEvent = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EventType = table.Column<int>(type: "int", nullable: false),
                    Detail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ReferenceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SourceIp = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserByName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    CorporationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractAudits", x => x.ContractAuditId);
                    table.ForeignKey(
                        name: "FK_ContractAudits_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "ClientId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContractAudits_ContractClients_ContractClientId",
                        column: x => x.ContractClientId,
                        principalTable: "ContractClients",
                        principalColumn: "ContractClientId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContractAudits_Corporations_CorporationId",
                        column: x => x.CorporationId,
                        principalTable: "Corporations",
                        principalColumn: "CorporationId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ContractExempts",
                columns: table => new
                {
                    ContractExemptId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    ContractClientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DateExempt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateEnded = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Motivo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ControlContrato = table.Column<long>(type: "bigint", nullable: false),
                    ClientName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ClientDocument = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ContractAddress = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ContractPhone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CityName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ZoneName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PlanName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PlanAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserByName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    UserIdEnded = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserByNameEnded = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    CorporationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractExempts", x => x.ContractExemptId);
                    table.ForeignKey(
                        name: "FK_ContractExempts_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "ClientId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContractExempts_ContractClients_ContractClientId",
                        column: x => x.ContractClientId,
                        principalTable: "ContractClients",
                        principalColumn: "ContractClientId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContractExempts_Corporations_CorporationId",
                        column: x => x.CorporationId,
                        principalTable: "Corporations",
                        principalColumn: "CorporationId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ContractExonerateds",
                columns: table => new
                {
                    ContractExoneratedId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    DateExonerated = table.Column<DateTime>(type: "date", nullable: false),
                    ExoneratedControl = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: true),
                    ClientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContractClientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    YearNumber = table.Column<int>(type: "int", nullable: false),
                    MonthType = table.Column<int>(type: "int", nullable: false),
                    TaxRate = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PriceWithTax = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Billed = table.Column<bool>(type: "bit", nullable: false),
                    DateBilled = table.Column<DateTime>(type: "date", nullable: true),
                    CxCBillId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DateEnded = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Motivo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ControlContrato = table.Column<long>(type: "bigint", nullable: false),
                    ClientName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ClientDocument = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ContractAddress = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ContractPhone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CityName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ZoneName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PlanName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CorporationId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserByName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    UserIdEnded = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserByNameEnded = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractExonerateds", x => x.ContractExoneratedId);
                    table.ForeignKey(
                        name: "FK_ContractExonerateds_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "ClientId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContractExonerateds_ContractClients_ContractClientId",
                        column: x => x.ContractClientId,
                        principalTable: "ContractClients",
                        principalColumn: "ContractClientId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContractExonerateds_Corporations_CorporationId",
                        column: x => x.CorporationId,
                        principalTable: "Corporations",
                        principalColumn: "CorporationId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContractExonerateds_CxCBills_CxCBillId",
                        column: x => x.CxCBillId,
                        principalTable: "CxCBills",
                        principalColumn: "CxCBillId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContractExonerateds_Plans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "Plans",
                        principalColumn: "PlanId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContractAudits_ClientId",
                table: "ContractAudits",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractAudits_ContractClientId_DateEvent",
                table: "ContractAudits",
                columns: new[] { "ContractClientId", "DateEvent" });

            migrationBuilder.CreateIndex(
                name: "IX_ContractAudits_CorporationId_DateEvent",
                table: "ContractAudits",
                columns: new[] { "CorporationId", "DateEvent" });

            migrationBuilder.CreateIndex(
                name: "IX_ContractExempts_ClientId",
                table: "ContractExempts",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractExempts_ContractClientId_DateEnded",
                table: "ContractExempts",
                columns: new[] { "ContractClientId", "DateEnded" });

            migrationBuilder.CreateIndex(
                name: "IX_ContractExempts_CorporationId_DateExempt",
                table: "ContractExempts",
                columns: new[] { "CorporationId", "DateExempt" });

            migrationBuilder.CreateIndex(
                name: "IX_ContractExonerateds_ClientId",
                table: "ContractExonerateds",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractExonerateds_ContractClientId",
                table: "ContractExonerateds",
                column: "ContractClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractExonerateds_CorporationId_ContractClientId_YearNumber_MonthType",
                table: "ContractExonerateds",
                columns: new[] { "CorporationId", "ContractClientId", "YearNumber", "MonthType" },
                unique: true,
                filter: "[DateEnded] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ContractExonerateds_CxCBillId",
                table: "ContractExonerateds",
                column: "CxCBillId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractExonerateds_PlanId",
                table: "ContractExonerateds",
                column: "PlanId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContractAudits");

            migrationBuilder.DropTable(
                name: "ContractExempts");

            migrationBuilder.DropTable(
                name: "ContractExonerateds");

            migrationBuilder.CreateTable(
                name: "PreExonerateds",
                columns: table => new
                {
                    PreExoneratedId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    ClientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContractClientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CorporationId = table.Column<int>(type: "int", nullable: false),
                    CxCBillId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Billed = table.Column<bool>(type: "bit", nullable: false),
                    DateBilled = table.Column<DateTime>(type: "date", nullable: true),
                    DateExonerated = table.Column<DateTime>(type: "date", nullable: false),
                    ExoneratedControl = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: true),
                    MonthType = table.Column<int>(type: "int", nullable: false),
                    PriceWithTax = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TaxRate = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UsuarioOwner = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    YearNumber = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreExonerateds", x => x.PreExoneratedId);
                    table.ForeignKey(
                        name: "FK_PreExonerateds_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "ClientId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PreExonerateds_ContractClients_ContractClientId",
                        column: x => x.ContractClientId,
                        principalTable: "ContractClients",
                        principalColumn: "ContractClientId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PreExonerateds_Corporations_CorporationId",
                        column: x => x.CorporationId,
                        principalTable: "Corporations",
                        principalColumn: "CorporationId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PreExonerateds_CxCBills_CxCBillId",
                        column: x => x.CxCBillId,
                        principalTable: "CxCBills",
                        principalColumn: "CxCBillId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PreExonerateds_Plans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "Plans",
                        principalColumn: "PlanId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PreExonerateds_ClientId",
                table: "PreExonerateds",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_PreExonerateds_ContractClientId",
                table: "PreExonerateds",
                column: "ContractClientId");

            migrationBuilder.CreateIndex(
                name: "IX_PreExonerateds_CorporationId_ContractClientId_YearNumber_MonthType",
                table: "PreExonerateds",
                columns: new[] { "CorporationId", "ContractClientId", "YearNumber", "MonthType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PreExonerateds_CxCBillId",
                table: "PreExonerateds",
                column: "CxCBillId");

            migrationBuilder.CreateIndex(
                name: "IX_PreExonerateds_PlanId",
                table: "PreExonerateds",
                column: "PlanId");
        }
    }
}
