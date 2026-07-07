using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spix.AppBacken.Migrations
{
    /// <inheritdoc />
    public partial class NewEmailProviderSettingDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "FileName",
                table: "ContractDocumentTemplates",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(120)",
                oldMaxLength: 120);

            migrationBuilder.CreateTable(
                name: "EmailProviderSettings",
                columns: table => new
                {
                    EmailProviderSettingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    Name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    ProviderType = table.Column<int>(type: "int", nullable: false),
                    FromEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    FromName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    SendGridApiKeyEncrypted = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    SmtpHost = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    SmtpPort = table.Column<int>(type: "int", nullable: true),
                    SmtpUseSsl = table.Column<bool>(type: "bit", nullable: false),
                    SmtpUser = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    SmtpPasswordEncrypted = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "date", nullable: false),
                    UsuarioOwner = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CorporationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailProviderSettings", x => x.EmailProviderSettingId);
                    table.ForeignKey(
                        name: "FK_EmailProviderSettings_Corporations_CorporationId",
                        column: x => x.CorporationId,
                        principalTable: "Corporations",
                        principalColumn: "CorporationId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmailProviderSettings_CorporationId_ProviderType_Name",
                table: "EmailProviderSettings",
                columns: new[] { "CorporationId", "ProviderType", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailProviderSettings");

            migrationBuilder.AlterColumn<string>(
                name: "FileName",
                table: "ContractDocumentTemplates",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(120)",
                oldMaxLength: 120,
                oldNullable: true);
        }
    }
}
