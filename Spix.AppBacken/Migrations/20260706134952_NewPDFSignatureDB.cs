using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spix.AppBacken.Migrations
{
    /// <inheritdoc />
    public partial class NewPDFSignatureDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContractDocumentTemplates",
                columns: table => new
                {
                    ContractDocumentTemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DocumentType = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: true),
                    PageCount = table.Column<int>(type: "int", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "date", nullable: false),
                    CorporationId = table.Column<int>(type: "int", nullable: false),
                    UsuarioOwner = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractDocumentTemplates", x => x.ContractDocumentTemplateId);
                    table.ForeignKey(
                        name: "FK_ContractDocumentTemplates_Corporations_CorporationId",
                        column: x => x.CorporationId,
                        principalTable: "Corporations",
                        principalColumn: "CorporationId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ContractDocumentTemplateFields",
                columns: table => new
                {
                    ContractDocumentTemplateFieldId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    ContractDocumentTemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FieldType = table.Column<int>(type: "int", nullable: false),
                    PageNumber = table.Column<int>(type: "int", nullable: false),
                    PositionX = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    PositionY = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Width = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    Height = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    FontSize = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractDocumentTemplateFields", x => x.ContractDocumentTemplateFieldId);
                    table.ForeignKey(
                        name: "FK_ContractDocumentTemplateFields_ContractDocumentTemplates_ContractDocumentTemplateId",
                        column: x => x.ContractDocumentTemplateId,
                        principalTable: "ContractDocumentTemplates",
                        principalColumn: "ContractDocumentTemplateId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ContractSignedDocuments",
                columns: table => new
                {
                    ContractSignedDocumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    ContractClientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContractDocumentTemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentType = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Signed = table.Column<bool>(type: "bit", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "date", nullable: false),
                    DateSigned = table.Column<DateTime>(type: "date", nullable: true),
                    CorporationId = table.Column<int>(type: "int", nullable: false),
                    UsuarioOwner = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UsuarioOwnerSigned = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserIdSigned = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractSignedDocuments", x => x.ContractSignedDocumentId);
                    table.ForeignKey(
                        name: "FK_ContractSignedDocuments_ContractClients_ContractClientId",
                        column: x => x.ContractClientId,
                        principalTable: "ContractClients",
                        principalColumn: "ContractClientId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContractSignedDocuments_ContractDocumentTemplates_ContractDocumentTemplateId",
                        column: x => x.ContractDocumentTemplateId,
                        principalTable: "ContractDocumentTemplates",
                        principalColumn: "ContractDocumentTemplateId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContractSignedDocuments_Corporations_CorporationId",
                        column: x => x.CorporationId,
                        principalTable: "Corporations",
                        principalColumn: "CorporationId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContractDocumentTemplateFields_ContractDocumentTemplateId",
                table: "ContractDocumentTemplateFields",
                column: "ContractDocumentTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractDocumentTemplates_CorporationId_DocumentType_Name",
                table: "ContractDocumentTemplates",
                columns: new[] { "CorporationId", "DocumentType", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContractSignedDocuments_ContractClientId_ContractDocumentTemplateId",
                table: "ContractSignedDocuments",
                columns: new[] { "ContractClientId", "ContractDocumentTemplateId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContractSignedDocuments_ContractDocumentTemplateId",
                table: "ContractSignedDocuments",
                column: "ContractDocumentTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractSignedDocuments_CorporationId",
                table: "ContractSignedDocuments",
                column: "CorporationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContractDocumentTemplateFields");

            migrationBuilder.DropTable(
                name: "ContractSignedDocuments");

            migrationBuilder.DropTable(
                name: "ContractDocumentTemplates");
        }
    }
}
