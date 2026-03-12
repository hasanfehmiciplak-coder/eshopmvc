using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EShopMVC.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatedByToOrderTimeline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "OrderTimelines",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "OrderTimelines");
        }
    }
}
