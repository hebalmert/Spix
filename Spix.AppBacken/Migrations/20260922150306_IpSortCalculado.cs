using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spix.AppBacken.Migrations
{
    /// <inheritdoc />
    public partial class IpSortCalculado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "IpSort",
                table: "IpNetworks",
                type: "bigint",
                nullable: true,
                computedColumnSql: "TRY_CAST(PARSENAME([Ip], 4) AS bigint) * 16777216 + TRY_CAST(PARSENAME([Ip], 3) AS bigint) * 65536 + TRY_CAST(PARSENAME([Ip], 2) AS bigint) * 256 + TRY_CAST(PARSENAME([Ip], 1) AS bigint)",
                stored: false);

            migrationBuilder.AddColumn<long>(
                name: "IpSort",
                table: "IpNets",
                type: "bigint",
                nullable: true,
                computedColumnSql: "TRY_CAST(PARSENAME([Ip], 4) AS bigint) * 16777216 + TRY_CAST(PARSENAME([Ip], 3) AS bigint) * 65536 + TRY_CAST(PARSENAME([Ip], 2) AS bigint) * 256 + TRY_CAST(PARSENAME([Ip], 1) AS bigint)",
                stored: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IpSort",
                table: "IpNetworks");

            migrationBuilder.DropColumn(
                name: "IpSort",
                table: "IpNets");
        }
    }
}
