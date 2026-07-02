using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spix.AppBacken.Migrations
{
    /// <inheritdoc />
    public partial class UpdateModelOperation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Technicians_CorporationId_FirstName_LastName",
                table: "Technicians");

            migrationBuilder.DropIndex(
                name: "IX_Contractors_CorporationId_DocumentTypeId_Document",
                table: "Contractors");

            migrationBuilder.DropIndex(
                name: "IX_Contractors_CorporationId_FirstName_LastName",
                table: "Contractors");

            migrationBuilder.DropIndex(
                name: "IX_Clients_CorporationId_DocumentTypeId_Document",
                table: "Clients");

            migrationBuilder.DropIndex(
                name: "IX_Clients_CorporationId_FirstName_LastName",
                table: "Clients");

            migrationBuilder.AlterColumn<int>(
                name: "Origin",
                table: "ScheduleItems",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_Contractors_CorporationId_FirstName_LastName_Document",
                table: "Contractors",
                columns: new[] { "CorporationId", "FirstName", "LastName", "Document" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clients_CorporationId_FirstName_LastName_Document",
                table: "Clients",
                columns: new[] { "CorporationId", "FirstName", "LastName", "Document" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Contractors_CorporationId_FirstName_LastName_Document",
                table: "Contractors");

            migrationBuilder.DropIndex(
                name: "IX_Clients_CorporationId_FirstName_LastName_Document",
                table: "Clients");

            migrationBuilder.AlterColumn<int>(
                name: "Origin",
                table: "ScheduleItems",
                type: "int",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_Technicians_CorporationId_FirstName_LastName",
                table: "Technicians",
                columns: new[] { "CorporationId", "FirstName", "LastName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Contractors_CorporationId_DocumentTypeId_Document",
                table: "Contractors",
                columns: new[] { "CorporationId", "DocumentTypeId", "Document" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Contractors_CorporationId_FirstName_LastName",
                table: "Contractors",
                columns: new[] { "CorporationId", "FirstName", "LastName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clients_CorporationId_DocumentTypeId_Document",
                table: "Clients",
                columns: new[] { "CorporationId", "DocumentTypeId", "Document" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clients_CorporationId_FirstName_LastName",
                table: "Clients",
                columns: new[] { "CorporationId", "FirstName", "LastName" },
                unique: true);
        }
    }
}
