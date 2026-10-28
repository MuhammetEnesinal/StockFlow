using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLimitConstraintsAndMaxLengths : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "RefreshTokenHash",
                table: "Users",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "Users",
                type: "nvarchar(60)",
                maxLength: 60,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "PurchaseOrderNumber",
                table: "PurchaseOrders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "OrderNumber",
                table: "Orders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "PerformedByUserName",
                table: "AuditLogs",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "PerformedByEmployeeCode",
                table: "AuditLogs",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_StockMovements_Quantity_Range",
                table: "StockMovements",
                sql: "[Quantity] <> 0 AND [Quantity] >= -1000000 AND [Quantity] <= 1000000");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PurchaseOrderItems_OrderedQuantity_Range",
                table: "PurchaseOrderItems",
                sql: "[OrderedQuantity] >= 1 AND [OrderedQuantity] <= 1000000");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PurchaseOrderItems_ReceivedQuantity_Range",
                table: "PurchaseOrderItems",
                sql: "[ReceivedQuantity] >= 0 AND [ReceivedQuantity] <= [OrderedQuantity]");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PurchaseOrderItems_UnitPrice_Range",
                table: "PurchaseOrderItems",
                sql: "[UnitPrice] > 0 AND [UnitPrice] <= 10000000");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Products_MinimumStockLevel_Range",
                table: "Products",
                sql: "[MinimumStockLevel] >= 0 AND [MinimumStockLevel] <= 1000000");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Products_Price_Range",
                table: "Products",
                sql: "[Price] > 0 AND [Price] <= 10000000");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Orders_TotalAmount_Range",
                table: "Orders",
                sql: "[TotalAmount] >= 0 AND [TotalAmount] <= 1000000000000000");

            migrationBuilder.AddCheckConstraint(
                name: "CK_OrderItems_Quantity_Range",
                table: "OrderItems",
                sql: "[Quantity] >= 1 AND [Quantity] <= 1000000");

            migrationBuilder.AddCheckConstraint(
                name: "CK_OrderItems_UnitPrice_Range",
                table: "OrderItems",
                sql: "[UnitPrice] > 0 AND [UnitPrice] <= 10000000");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_StockMovements_Quantity_Range",
                table: "StockMovements");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PurchaseOrderItems_OrderedQuantity_Range",
                table: "PurchaseOrderItems");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PurchaseOrderItems_ReceivedQuantity_Range",
                table: "PurchaseOrderItems");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PurchaseOrderItems_UnitPrice_Range",
                table: "PurchaseOrderItems");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Products_MinimumStockLevel_Range",
                table: "Products");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Products_Price_Range",
                table: "Products");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Orders_TotalAmount_Range",
                table: "Orders");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OrderItems_Quantity_Range",
                table: "OrderItems");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OrderItems_UnitPrice_Range",
                table: "OrderItems");

            migrationBuilder.AlterColumn<string>(
                name: "RefreshTokenHash",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(60)",
                oldMaxLength: 60);

            migrationBuilder.AlterColumn<string>(
                name: "PurchaseOrderNumber",
                table: "PurchaseOrders",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "OrderNumber",
                table: "Orders",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "PerformedByUserName",
                table: "AuditLogs",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "PerformedByEmployeeCode",
                table: "AuditLogs",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);
        }
    }
}
