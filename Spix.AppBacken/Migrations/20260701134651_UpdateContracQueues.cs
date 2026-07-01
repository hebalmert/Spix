using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spix.AppBacken.Migrations
{
    /// <inheritdoc />
    public partial class UpdateContracQueues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "QueueParents",
                columns: table => new
                {
                    QueueParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    ParentName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ServerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Down = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    Up = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    MkId = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    CorporationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QueueParents", x => x.QueueParentId);
                    table.ForeignKey(
                        name: "FK_QueueParents_Corporations_CorporationId",
                        column: x => x.CorporationId,
                        principalTable: "Corporations",
                        principalColumn: "CorporationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QueueParents_Plans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "Plans",
                        principalColumn: "PlanId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QueueParents_Servers_ServerId",
                        column: x => x.ServerId,
                        principalTable: "Servers",
                        principalColumn: "ServerId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QueueTypes",
                columns: table => new
                {
                    QueueTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    TypeName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Down = table.Column<bool>(type: "bit", nullable: false),
                    Up = table.Column<bool>(type: "bit", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    CorporationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QueueTypes", x => x.QueueTypeId);
                    table.ForeignKey(
                        name: "FK_QueueTypes_Corporations_CorporationId",
                        column: x => x.CorporationId,
                        principalTable: "Corporations",
                        principalColumn: "CorporationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QueueParents_CorporationId_ParentName_ServerId",
                table: "QueueParents",
                columns: new[] { "CorporationId", "ParentName", "ServerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QueueParents_PlanId",
                table: "QueueParents",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_QueueParents_ServerId",
                table: "QueueParents",
                column: "ServerId");

            migrationBuilder.CreateIndex(
                name: "IX_QueueTypes_CorporationId_TypeName",
                table: "QueueTypes",
                columns: new[] { "CorporationId", "TypeName" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QueueParents");

            migrationBuilder.DropTable(
                name: "QueueTypes");
        }
    }
}
