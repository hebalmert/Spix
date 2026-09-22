using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spix.AppBacken.Migrations
{
    /// <inheritdoc />
    public partial class IpSortGuardadoUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_IpNetworks_CorporationId",
                table: "IpNetworks");

            migrationBuilder.DropIndex(
                name: "IX_IpNets_CorporationId",
                table: "IpNets");

            migrationBuilder.AlterColumn<long>(
                name: "IpSort",
                table: "IpNetworks",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true,
                oldComputedColumnSql: "TRY_CAST(PARSENAME([Ip], 4) AS bigint) * 16777216 + TRY_CAST(PARSENAME([Ip], 3) AS bigint) * 65536 + TRY_CAST(PARSENAME([Ip], 2) AS bigint) * 256 + TRY_CAST(PARSENAME([Ip], 1) AS bigint)");

            migrationBuilder.AlterColumn<long>(
                name: "IpSort",
                table: "IpNets",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true,
                oldComputedColumnSql: "TRY_CAST(PARSENAME([Ip], 4) AS bigint) * 16777216 + TRY_CAST(PARSENAME([Ip], 3) AS bigint) * 65536 + TRY_CAST(PARSENAME([Ip], 2) AS bigint) * 256 + TRY_CAST(PARSENAME([Ip], 1) AS bigint)");

            //Las IP que ya existen: se les calcula la clave de orden una sola vez.
            //Desde aqui la calcula la entidad al asignar la IP (IpSortKey).
            migrationBuilder.Sql(@"
UPDATE IpNets
SET IpSort = TRY_CAST(PARSENAME([Ip], 4) AS bigint) * 16777216
           + TRY_CAST(PARSENAME([Ip], 3) AS bigint) * 65536
           + TRY_CAST(PARSENAME([Ip], 2) AS bigint) * 256
           + TRY_CAST(PARSENAME([Ip], 1) AS bigint);

UPDATE IpNetworks
SET IpSort = TRY_CAST(PARSENAME([Ip], 4) AS bigint) * 16777216
           + TRY_CAST(PARSENAME([Ip], 3) AS bigint) * 65536
           + TRY_CAST(PARSENAME([Ip], 2) AS bigint) * 256
           + TRY_CAST(PARSENAME([Ip], 1) AS bigint);");

            migrationBuilder.CreateIndex(
                name: "IX_IpNetworks_CorporationId_IpSort",
                table: "IpNetworks",
                columns: new[] { "CorporationId", "IpSort" });

            migrationBuilder.CreateIndex(
                name: "IX_IpNets_CorporationId_IpSort",
                table: "IpNets",
                columns: new[] { "CorporationId", "IpSort" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_IpNetworks_CorporationId_IpSort",
                table: "IpNetworks");

            migrationBuilder.DropIndex(
                name: "IX_IpNets_CorporationId_IpSort",
                table: "IpNets");

            migrationBuilder.AlterColumn<long>(
                name: "IpSort",
                table: "IpNetworks",
                type: "bigint",
                nullable: true,
                computedColumnSql: "TRY_CAST(PARSENAME([Ip], 4) AS bigint) * 16777216 + TRY_CAST(PARSENAME([Ip], 3) AS bigint) * 65536 + TRY_CAST(PARSENAME([Ip], 2) AS bigint) * 256 + TRY_CAST(PARSENAME([Ip], 1) AS bigint)",
                stored: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "IpSort",
                table: "IpNets",
                type: "bigint",
                nullable: true,
                computedColumnSql: "TRY_CAST(PARSENAME([Ip], 4) AS bigint) * 16777216 + TRY_CAST(PARSENAME([Ip], 3) AS bigint) * 65536 + TRY_CAST(PARSENAME([Ip], 2) AS bigint) * 256 + TRY_CAST(PARSENAME([Ip], 1) AS bigint)",
                stored: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_IpNetworks_CorporationId",
                table: "IpNetworks",
                column: "CorporationId");

            migrationBuilder.CreateIndex(
                name: "IX_IpNets_CorporationId",
                table: "IpNets",
                column: "CorporationId");
        }
    }
}
