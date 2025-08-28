using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortfolioApp.Infrastructure.EfCore.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class add_lot_purchase_date : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "version",
                table: "lots");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "version",
                table: "lots",
                type: "BLOB",
                rowVersion: true,
                nullable: true);
        }
    }
}
