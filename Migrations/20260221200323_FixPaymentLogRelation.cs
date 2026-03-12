using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EShopMVC.Migrations
{
    /// <inheritdoc />
    public partial class FixPaymentLogRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_PaymentLogs_PaymentLogId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_RefundLogs_Orders_OrderId",
                table: "RefundLogs");

            migrationBuilder.DropIndex(
                name: "IX_Orders_PaymentLogId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PaymentLogId",
                table: "Orders");

            migrationBuilder.AddColumn<int>(
                name: "OrderId1",
                table: "RefundLogs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CustomerEmail",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CustomerName",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_RefundLogs_OrderId1",
                table: "RefundLogs",
                column: "OrderId1");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentLogs_OrderId",
                table: "PaymentLogs",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderTimelines_OrderId",
                table: "OrderTimelines",
                column: "OrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderTimelines_Orders_OrderId",
                table: "OrderTimelines",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentLogs_Orders_OrderId",
                table: "PaymentLogs",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RefundLogs_Orders_OrderId",
                table: "RefundLogs",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RefundLogs_Orders_OrderId1",
                table: "RefundLogs",
                column: "OrderId1",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderTimelines_Orders_OrderId",
                table: "OrderTimelines");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentLogs_Orders_OrderId",
                table: "PaymentLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_RefundLogs_Orders_OrderId",
                table: "RefundLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_RefundLogs_Orders_OrderId1",
                table: "RefundLogs");

            migrationBuilder.DropIndex(
                name: "IX_RefundLogs_OrderId1",
                table: "RefundLogs");

            migrationBuilder.DropIndex(
                name: "IX_PaymentLogs_OrderId",
                table: "PaymentLogs");

            migrationBuilder.DropIndex(
                name: "IX_OrderTimelines_OrderId",
                table: "OrderTimelines");

            migrationBuilder.DropColumn(
                name: "OrderId1",
                table: "RefundLogs");

            migrationBuilder.DropColumn(
                name: "CustomerEmail",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CustomerName",
                table: "Orders");

            migrationBuilder.AddColumn<int>(
                name: "PaymentLogId",
                table: "Orders",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_PaymentLogId",
                table: "Orders",
                column: "PaymentLogId",
                unique: true,
                filter: "[PaymentLogId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_PaymentLogs_PaymentLogId",
                table: "Orders",
                column: "PaymentLogId",
                principalTable: "PaymentLogs",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_RefundLogs_Orders_OrderId",
                table: "RefundLogs",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
