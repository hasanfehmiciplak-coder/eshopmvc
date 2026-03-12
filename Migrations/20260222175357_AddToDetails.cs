using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EShopMVC.Migrations
{
    /// <inheritdoc />
    public partial class AddToDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Details",
                table: "OrderTimelines",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Details",
                table: "OrderTimelines");
        }
    }
}
