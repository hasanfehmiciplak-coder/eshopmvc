using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EShopMVC.Migrations
{
    /// <inheritdoc />
    public partial class AddCancelledAtToPartialRefund : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledAt",
                table: "PartialRefunds",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CancelledAt",
                table: "PartialRefunds");
        }
    }
}
