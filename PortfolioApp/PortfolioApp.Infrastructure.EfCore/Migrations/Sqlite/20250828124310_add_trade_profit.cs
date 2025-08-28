using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortfolioApp.Infrastructure.EfCore.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class add_trade_profit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ProfitGross",
                table: "trades",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ProfitNet",
                table: "trades",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfitGross",
                table: "trades");

            migrationBuilder.DropColumn(
                name: "ProfitNet",
                table: "trades");
        }
    }
}
