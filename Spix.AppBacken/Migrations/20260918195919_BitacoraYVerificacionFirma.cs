using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spix.AppBacken.Migrations
{
    /// <inheritdoc />
    public partial class BitacoraYVerificacionFirma : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "DateSigned",
                table: "ContractSignedDocuments",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConsentHash",
                table: "ContractSignedDocuments",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConsentVersion",
                table: "ContractSignedDocuments",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FileHash",
                table: "ContractSignedDocuments",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VerificationCode",
                table: "ContractSignedDocuments",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ContractSignatureEvents",
                columns: table => new
                {
                    ContractSignatureEventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    ContractClientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentType = table.Column<int>(type: "int", nullable: false),
                    EventType = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Detail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    SourceIp = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    CorporationId = table.Column<int>(type: "int", nullable: false),
                    UsuarioOwner = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractSignatureEvents", x => x.ContractSignatureEventId);
                    table.ForeignKey(
                        name: "FK_ContractSignatureEvents_ContractClients_ContractClientId",
                        column: x => x.ContractClientId,
                        principalTable: "ContractClients",
                        principalColumn: "ContractClientId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContractSignatureEvents_Corporations_CorporationId",
                        column: x => x.CorporationId,
                        principalTable: "Corporations",
                        principalColumn: "CorporationId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContractSignedDocuments_VerificationCode",
                table: "ContractSignedDocuments",
                column: "VerificationCode",
                unique: true,
                filter: "[VerificationCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ContractSignatureEvents_ContractClientId_DocumentType_CreatedAt",
                table: "ContractSignatureEvents",
                columns: new[] { "ContractClientId", "DocumentType", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ContractSignatureEvents_CorporationId",
                table: "ContractSignatureEvents",
                column: "CorporationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContractSignatureEvents");

            migrationBuilder.DropIndex(
                name: "IX_ContractSignedDocuments_VerificationCode",
                table: "ContractSignedDocuments");

            migrationBuilder.DropColumn(
                name: "ConsentHash",
                table: "ContractSignedDocuments");

            migrationBuilder.DropColumn(
                name: "ConsentVersion",
                table: "ContractSignedDocuments");

            migrationBuilder.DropColumn(
                name: "FileHash",
                table: "ContractSignedDocuments");

            migrationBuilder.DropColumn(
                name: "VerificationCode",
                table: "ContractSignedDocuments");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateSigned",
                table: "ContractSignedDocuments",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);
        }
    }
}
