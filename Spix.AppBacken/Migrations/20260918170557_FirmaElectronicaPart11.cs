using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spix.AppBacken.Migrations
{
    /// <inheritdoc />
    public partial class FirmaElectronicaPart11 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CodeSentAt",
                table: "ContractSignedDocuments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CodeValidatedAt",
                table: "ContractSignedDocuments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DocumentHash",
                table: "ContractSignedDocuments",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SignatureMethod",
                table: "ContractSignedDocuments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SignerEmail",
                table: "ContractSignedDocuments",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SignerIp",
                table: "ContractSignedDocuments",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SignerUserAgent",
                table: "ContractSignedDocuments",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TermsAcceptedAt",
                table: "ContractSignedDocuments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "WitnessUserId",
                table: "ContractSignedDocuments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WitnessUserName",
                table: "ContractSignedDocuments",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ContractSignatureCodes",
                columns: table => new
                {
                    ContractSignatureCodeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    ContractClientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentType = table.Column<int>(type: "int", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    CodeHash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Attempts = table.Column<int>(type: "int", nullable: false),
                    UsedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RequestIp = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    RequestUserAgent = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    CorporationId = table.Column<int>(type: "int", nullable: false),
                    UsuarioOwner = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractSignatureCodes", x => x.ContractSignatureCodeId);
                    table.ForeignKey(
                        name: "FK_ContractSignatureCodes_ContractClients_ContractClientId",
                        column: x => x.ContractClientId,
                        principalTable: "ContractClients",
                        principalColumn: "ContractClientId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContractSignatureCodes_Corporations_CorporationId",
                        column: x => x.CorporationId,
                        principalTable: "Corporations",
                        principalColumn: "CorporationId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContractSignatureCodes_ContractClientId_DocumentType_CreatedAt",
                table: "ContractSignatureCodes",
                columns: new[] { "ContractClientId", "DocumentType", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ContractSignatureCodes_CorporationId",
                table: "ContractSignatureCodes",
                column: "CorporationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContractSignatureCodes");

            migrationBuilder.DropColumn(
                name: "CodeSentAt",
                table: "ContractSignedDocuments");

            migrationBuilder.DropColumn(
                name: "CodeValidatedAt",
                table: "ContractSignedDocuments");

            migrationBuilder.DropColumn(
                name: "DocumentHash",
                table: "ContractSignedDocuments");

            migrationBuilder.DropColumn(
                name: "SignatureMethod",
                table: "ContractSignedDocuments");

            migrationBuilder.DropColumn(
                name: "SignerEmail",
                table: "ContractSignedDocuments");

            migrationBuilder.DropColumn(
                name: "SignerIp",
                table: "ContractSignedDocuments");

            migrationBuilder.DropColumn(
                name: "SignerUserAgent",
                table: "ContractSignedDocuments");

            migrationBuilder.DropColumn(
                name: "TermsAcceptedAt",
                table: "ContractSignedDocuments");

            migrationBuilder.DropColumn(
                name: "WitnessUserId",
                table: "ContractSignedDocuments");

            migrationBuilder.DropColumn(
                name: "WitnessUserName",
                table: "ContractSignedDocuments");
        }
    }
}
