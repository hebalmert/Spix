using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spix.AppBacken.Migrations
{
    /// <inheritdoc />
    public partial class NewSellBillingDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Billed",
                table: "ServiceRequests",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "SellId",
                table: "ServiceRequests",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "ServiceRequestDetails",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "SellDetailId",
                table: "ServiceRequestDetails",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TaxAmount",
                table: "ServiceRequestDetails",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "TaxId",
                table: "ServiceRequestDetails",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TaxRate",
                table: "ServiceRequestDetails",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "BillingNoteOnes",
                columns: table => new
                {
                    BillingNoteOneId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    DateBill = table.Column<DateTime>(type: "date", nullable: false),
                    ClientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContractClientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    YearNumber = table.Column<int>(type: "int", nullable: false),
                    MonthType = table.Column<int>(type: "int", nullable: false),
                    CorporationId = table.Column<int>(type: "int", nullable: false),
                    UsuarioOwner = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BillingNoteOnes", x => x.BillingNoteOneId);
                    table.ForeignKey(
                        name: "FK_BillingNoteOnes_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "ClientId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BillingNoteOnes_ContractClients_ContractClientId",
                        column: x => x.ContractClientId,
                        principalTable: "ContractClients",
                        principalColumn: "ContractClientId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BillingNoteOnes_Corporations_CorporationId",
                        column: x => x.CorporationId,
                        principalTable: "Corporations",
                        principalColumn: "CorporationId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BillingNotes",
                columns: table => new
                {
                    BillingNoteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    DateBill = table.Column<DateTime>(type: "date", nullable: false),
                    YearNumber = table.Column<int>(type: "int", nullable: false),
                    MonthType = table.Column<int>(type: "int", nullable: false),
                    Created = table.Column<bool>(type: "bit", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "date", nullable: true),
                    CorporationId = table.Column<int>(type: "int", nullable: false),
                    UsuarioOwner = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BillingNotes", x => x.BillingNoteId);
                    table.ForeignKey(
                        name: "FK_BillingNotes_Corporations_CorporationId",
                        column: x => x.CorporationId,
                        principalTable: "Corporations",
                        principalColumn: "CorporationId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Sells",
                columns: table => new
                {
                    SellId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    DateSell = table.Column<DateTime>(type: "date", nullable: false),
                    InvoiceNumber = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: true),
                    ContractClientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ControlContrato = table.Column<long>(type: "bigint", nullable: false),
                    ClientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClientFullName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    DocumentTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DocumentTypeName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Identification = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ZoneName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Cancelled = table.Column<bool>(type: "bit", nullable: false),
                    DateCancelled = table.Column<DateTime>(type: "date", nullable: true),
                    Printed = table.Column<bool>(type: "bit", nullable: false),
                    Paid = table.Column<bool>(type: "bit", nullable: false),
                    DatePaid = table.Column<DateTime>(type: "date", nullable: true),
                    BillingNoteId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BillingNoteOneId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CorporationId = table.Column<int>(type: "int", nullable: false),
                    UsuarioOwner = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sells", x => x.SellId);
                    table.ForeignKey(
                        name: "FK_Sells_BillingNoteOnes_BillingNoteOneId",
                        column: x => x.BillingNoteOneId,
                        principalTable: "BillingNoteOnes",
                        principalColumn: "BillingNoteOneId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Sells_BillingNotes_BillingNoteId",
                        column: x => x.BillingNoteId,
                        principalTable: "BillingNotes",
                        principalColumn: "BillingNoteId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Sells_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "ClientId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Sells_ContractClients_ContractClientId",
                        column: x => x.ContractClientId,
                        principalTable: "ContractClients",
                        principalColumn: "ContractClientId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Sells_Corporations_CorporationId",
                        column: x => x.CorporationId,
                        principalTable: "Corporations",
                        principalColumn: "CorporationId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CxCBills",
                columns: table => new
                {
                    CxCBillId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    DateNote = table.Column<DateTime>(type: "date", nullable: false),
                    CollectionNote = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: true),
                    ClientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContractClientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Balance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SellId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BillingNoteOneId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Paid = table.Column<bool>(type: "bit", nullable: false),
                    DatePaid = table.Column<DateTime>(type: "date", nullable: true),
                    Cancelled = table.Column<bool>(type: "bit", nullable: false),
                    DateCancelled = table.Column<DateTime>(type: "date", nullable: true),
                    CorporationId = table.Column<int>(type: "int", nullable: false),
                    UsuarioOwner = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CxCBills", x => x.CxCBillId);
                    table.ForeignKey(
                        name: "FK_CxCBills_BillingNoteOnes_BillingNoteOneId",
                        column: x => x.BillingNoteOneId,
                        principalTable: "BillingNoteOnes",
                        principalColumn: "BillingNoteOneId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CxCBills_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "ClientId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CxCBills_ContractClients_ContractClientId",
                        column: x => x.ContractClientId,
                        principalTable: "ContractClients",
                        principalColumn: "ContractClientId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CxCBills_Corporations_CorporationId",
                        column: x => x.CorporationId,
                        principalTable: "Corporations",
                        principalColumn: "CorporationId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CxCBills_Sells_SellId",
                        column: x => x.SellId,
                        principalTable: "Sells",
                        principalColumn: "SellId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SellDetails",
                columns: table => new
                {
                    SellDetailId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    SellId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Origin = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Concept = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TaxId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TaxRate = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ServiceRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CorporationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SellDetails", x => x.SellDetailId);
                    table.ForeignKey(
                        name: "FK_SellDetails_Corporations_CorporationId",
                        column: x => x.CorporationId,
                        principalTable: "Corporations",
                        principalColumn: "CorporationId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SellDetails_Sells_SellId",
                        column: x => x.SellId,
                        principalTable: "Sells",
                        principalColumn: "SellId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SellDetails_ServiceRequests_ServiceRequestId",
                        column: x => x.ServiceRequestId,
                        principalTable: "ServiceRequests",
                        principalColumn: "ServiceRequestId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SellDetails_Taxes_TaxId",
                        column: x => x.TaxId,
                        principalTable: "Taxes",
                        principalColumn: "TaxId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CxCBillDetails",
                columns: table => new
                {
                    CxCBillDetailId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CxCBillId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DatePayment = table.Column<DateTime>(type: "date", nullable: false),
                    PaymentMode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    DiscountRate = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Detail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Debt = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Payment = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Discount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Balance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CorporationId = table.Column<int>(type: "int", nullable: false),
                    UsuarioOwner = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CxCBillDetails", x => x.CxCBillDetailId);
                    table.ForeignKey(
                        name: "FK_CxCBillDetails_Corporations_CorporationId",
                        column: x => x.CorporationId,
                        principalTable: "Corporations",
                        principalColumn: "CorporationId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CxCBillDetails_CxCBills_CxCBillId",
                        column: x => x.CxCBillId,
                        principalTable: "CxCBills",
                        principalColumn: "CxCBillId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_SellId",
                table: "ServiceRequests",
                column: "SellId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequestDetails_SellDetailId",
                table: "ServiceRequestDetails",
                column: "SellDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequestDetails_TaxId",
                table: "ServiceRequestDetails",
                column: "TaxId");

            migrationBuilder.CreateIndex(
                name: "IX_BillingNoteOnes_ClientId",
                table: "BillingNoteOnes",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_BillingNoteOnes_ContractClientId",
                table: "BillingNoteOnes",
                column: "ContractClientId");

            migrationBuilder.CreateIndex(
                name: "IX_BillingNoteOnes_CorporationId_ContractClientId_YearNumber_MonthType",
                table: "BillingNoteOnes",
                columns: new[] { "CorporationId", "ContractClientId", "YearNumber", "MonthType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BillingNotes_CorporationId_YearNumber_MonthType",
                table: "BillingNotes",
                columns: new[] { "CorporationId", "YearNumber", "MonthType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CxCBillDetails_CorporationId",
                table: "CxCBillDetails",
                column: "CorporationId");

            migrationBuilder.CreateIndex(
                name: "IX_CxCBillDetails_CxCBillId",
                table: "CxCBillDetails",
                column: "CxCBillId");

            migrationBuilder.CreateIndex(
                name: "IX_CxCBills_BillingNoteOneId",
                table: "CxCBills",
                column: "BillingNoteOneId");

            migrationBuilder.CreateIndex(
                name: "IX_CxCBills_ClientId",
                table: "CxCBills",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_CxCBills_ContractClientId",
                table: "CxCBills",
                column: "ContractClientId");

            migrationBuilder.CreateIndex(
                name: "IX_CxCBills_CorporationId_CollectionNote",
                table: "CxCBills",
                columns: new[] { "CorporationId", "CollectionNote" },
                unique: true,
                filter: "[CollectionNote] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CxCBills_SellId",
                table: "CxCBills",
                column: "SellId");

            migrationBuilder.CreateIndex(
                name: "IX_SellDetails_CorporationId",
                table: "SellDetails",
                column: "CorporationId");

            migrationBuilder.CreateIndex(
                name: "IX_SellDetails_SellId",
                table: "SellDetails",
                column: "SellId");

            migrationBuilder.CreateIndex(
                name: "IX_SellDetails_ServiceRequestId",
                table: "SellDetails",
                column: "ServiceRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_SellDetails_TaxId",
                table: "SellDetails",
                column: "TaxId");

            migrationBuilder.CreateIndex(
                name: "IX_Sells_BillingNoteId",
                table: "Sells",
                column: "BillingNoteId");

            migrationBuilder.CreateIndex(
                name: "IX_Sells_BillingNoteOneId",
                table: "Sells",
                column: "BillingNoteOneId");

            migrationBuilder.CreateIndex(
                name: "IX_Sells_ClientId",
                table: "Sells",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Sells_ContractClientId",
                table: "Sells",
                column: "ContractClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Sells_CorporationId_InvoiceNumber",
                table: "Sells",
                columns: new[] { "CorporationId", "InvoiceNumber" },
                unique: true,
                filter: "[InvoiceNumber] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceRequestDetails_SellDetails_SellDetailId",
                table: "ServiceRequestDetails",
                column: "SellDetailId",
                principalTable: "SellDetails",
                principalColumn: "SellDetailId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceRequestDetails_Taxes_TaxId",
                table: "ServiceRequestDetails",
                column: "TaxId",
                principalTable: "Taxes",
                principalColumn: "TaxId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceRequests_Sells_SellId",
                table: "ServiceRequests",
                column: "SellId",
                principalTable: "Sells",
                principalColumn: "SellId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceRequestDetails_SellDetails_SellDetailId",
                table: "ServiceRequestDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceRequestDetails_Taxes_TaxId",
                table: "ServiceRequestDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceRequests_Sells_SellId",
                table: "ServiceRequests");

            migrationBuilder.DropTable(
                name: "CxCBillDetails");

            migrationBuilder.DropTable(
                name: "SellDetails");

            migrationBuilder.DropTable(
                name: "CxCBills");

            migrationBuilder.DropTable(
                name: "Sells");

            migrationBuilder.DropTable(
                name: "BillingNoteOnes");

            migrationBuilder.DropTable(
                name: "BillingNotes");

            migrationBuilder.DropIndex(
                name: "IX_ServiceRequests_SellId",
                table: "ServiceRequests");

            migrationBuilder.DropIndex(
                name: "IX_ServiceRequestDetails_SellDetailId",
                table: "ServiceRequestDetails");

            migrationBuilder.DropIndex(
                name: "IX_ServiceRequestDetails_TaxId",
                table: "ServiceRequestDetails");

            migrationBuilder.DropColumn(
                name: "Billed",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "SellId",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "ServiceRequestDetails");

            migrationBuilder.DropColumn(
                name: "SellDetailId",
                table: "ServiceRequestDetails");

            migrationBuilder.DropColumn(
                name: "TaxAmount",
                table: "ServiceRequestDetails");

            migrationBuilder.DropColumn(
                name: "TaxId",
                table: "ServiceRequestDetails");

            migrationBuilder.DropColumn(
                name: "TaxRate",
                table: "ServiceRequestDetails");
        }
    }
}
