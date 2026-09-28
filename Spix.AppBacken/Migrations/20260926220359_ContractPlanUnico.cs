using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spix.AppBacken.Migrations
{
    /// <inheritdoc />
    public partial class ContractPlanUnico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ContractPlans_ContractClientId_PlanId",
                table: "ContractPlans");

            migrationBuilder.CreateIndex(
                name: "IX_ContractPlans_ContractClientId",
                table: "ContractPlans",
                column: "ContractClientId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ContractPlans_ContractClientId",
                table: "ContractPlans");

            migrationBuilder.CreateIndex(
                name: "IX_ContractPlans_ContractClientId_PlanId",
                table: "ContractPlans",
                columns: new[] { "ContractClientId", "PlanId" },
                unique: true);
        }
    }
}
