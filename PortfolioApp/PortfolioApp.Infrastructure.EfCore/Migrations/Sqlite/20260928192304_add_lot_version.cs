using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortfolioApp.Infrastructure.EfCore.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class add_lot_version : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "lots",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Version",
                table: "lots");
        }
    }
}
