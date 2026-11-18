using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POSMT.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class Phase6MultiTerminalSales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Sales_Amounts",
                table: "Sales");

            migrationBuilder.DropCheckConstraint(
                name: "CK_SaleItems_Quantity",
                table: "SaleItems");

            migrationBuilder.AddColumn<int>(
                name: "TerminalId",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "PaidAt",
                table: "Payments",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<string>(
                name: "PaymentStatus",
                table: "Payments",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Pending");

            migrationBuilder.CreateIndex(
                name: "IX_Users_TerminalId",
                table: "Users",
                column: "TerminalId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Terminals_Status",
                table: "Terminals",
                sql: "[Status] IN ('Active', 'Inactive')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Sales_Amounts",
                table: "Sales",
                sql: "[Subtotal] >= 0 AND [Discount] >= 0 AND [Discount] <= [Subtotal] AND [TotalAmount] = [Subtotal] - [Discount]");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Sales_Status",
                table: "Sales",
                sql: "[Status] IN ('Pending', 'Completed', 'Cancelled')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_SaleItems_Quantity",
                table: "SaleItems",
                sql: "[Quantity] > 0 AND [UnitPrice] >= 0 AND [LineTotal] >= 0 AND [LineTotal] = [Quantity] * [UnitPrice]");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Payments_Amount",
                table: "Payments",
                sql: "[Amount] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Payments_Method",
                table: "Payments",
                sql: "[PaymentMethod] IN ('Cash', 'Card')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Payments_Status",
                table: "Payments",
                sql: "[PaymentStatus] IN ('Pending', 'Completed', 'Failed', 'Refunded')");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Terminals_TerminalId",
                table: "Users",
                column: "TerminalId",
                principalTable: "Terminals",
                principalColumn: "TerminalId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Terminals_TerminalId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_TerminalId",
                table: "Users");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Terminals_Status",
                table: "Terminals");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Sales_Amounts",
                table: "Sales");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Sales_Status",
                table: "Sales");

            migrationBuilder.DropCheckConstraint(
                name: "CK_SaleItems_Quantity",
                table: "SaleItems");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Payments_Amount",
                table: "Payments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Payments_Method",
                table: "Payments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Payments_Status",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "TerminalId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                table: "Payments");

            migrationBuilder.AlterColumn<DateTime>(
                name: "PaidAt",
                table: "Payments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Sales_Amounts",
                table: "Sales",
                sql: "[Subtotal] >= 0 AND [Discount] >= 0 AND [Discount] <= [Subtotal] AND [TotalAmount] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_SaleItems_Quantity",
                table: "SaleItems",
                sql: "[Quantity] > 0");
        }
    }
}
