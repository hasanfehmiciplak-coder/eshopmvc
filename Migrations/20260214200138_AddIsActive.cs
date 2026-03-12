using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EShopMVC.Migrations
{
    /// <inheritdoc />
    public partial class AddIsActive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Amount",
                table: "Refunds",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledAt",
                table: "Refunds",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Refunds",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "OrderId",
                table: "Refunds",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OrderItemId",
                table: "Refunds",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "Refunds",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "RejectedByUserId",
                table: "PartialRefunds",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "FraudFlags",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Amount",
                table: "Refunds");

            migrationBuilder.DropColumn(
                name: "CancelledAt",
                table: "Refunds");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Refunds");

            migrationBuilder.DropColumn(
                name: "OrderId",
                table: "Refunds");

            migrationBuilder.DropColumn(
                name: "OrderItemId",
                table: "Refunds");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "Refunds");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "FraudFlags");

            migrationBuilder.AlterColumn<string>(
                name: "RejectedByUserId",
                table: "PartialRefunds",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
