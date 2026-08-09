using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spix.AppBacken.Migrations
{
    /// <inheritdoc />
    public partial class SaasSubscriptionAndMercadoPago : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AnnualPrice",
                table: "SoftPlans",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "SoftPlans",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsRecommended",
                table: "SoftPlans",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PublicDescription",
                table: "SoftPlans",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CorporationSubscriptions",
                columns: table => new
                {
                    CorporationSubscriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CorporationId = table.Column<int>(type: "int", nullable: false),
                    SoftPlanId = table.Column<int>(type: "int", nullable: false),
                    Cycle = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DateCreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrialStartsUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TrialEndsUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CurrentPeriodStartsUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CurrentPeriodEndsUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExternalReference = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    MercadoPagoPreapprovalId = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    CheckoutUrl = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    UserModifiedByName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CorporationSubscriptions", x => x.CorporationSubscriptionId);
                    table.ForeignKey(
                        name: "FK_CorporationSubscriptions_Corporations_CorporationId",
                        column: x => x.CorporationId,
                        principalTable: "Corporations",
                        principalColumn: "CorporationId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CorporationSubscriptions_SoftPlans_SoftPlanId",
                        column: x => x.SoftPlanId,
                        principalTable: "SoftPlans",
                        principalColumn: "SoftPlanId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MercadoPagoPlatformSettings",
                columns: table => new
                {
                    MercadoPagoPlatformSettingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    PublicKeyEncrypted = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    AccessTokenEncrypted = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    WebhookSecretEncrypted = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    WebhookUrl = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    DateModifiedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserModifiedByName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MercadoPagoPlatformSettings", x => x.MercadoPagoPlatformSettingId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CorporationSubscriptions_CorporationId_Status",
                table: "CorporationSubscriptions",
                columns: new[] { "CorporationId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_CorporationSubscriptions_ExternalReference",
                table: "CorporationSubscriptions",
                column: "ExternalReference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CorporationSubscriptions_SoftPlanId",
                table: "CorporationSubscriptions",
                column: "SoftPlanId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CorporationSubscriptions");

            migrationBuilder.DropTable(
                name: "MercadoPagoPlatformSettings");

            migrationBuilder.DropColumn(
                name: "AnnualPrice",
                table: "SoftPlans");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "SoftPlans");

            migrationBuilder.DropColumn(
                name: "IsRecommended",
                table: "SoftPlans");

            migrationBuilder.DropColumn(
                name: "PublicDescription",
                table: "SoftPlans");
        }
    }
}
