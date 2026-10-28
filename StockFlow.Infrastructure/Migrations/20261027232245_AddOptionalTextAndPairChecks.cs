using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOptionalTextAndPairChecks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_Warehouses_Address_NotEmpty",
                table: "Warehouses",
                sql: "[Address] IS NULL OR LEN([Address]) > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Users_RefreshToken_Pair",
                table: "Users",
                sql: "([RefreshTokenHash] IS NULL AND [RefreshTokenExpiresAt] IS NULL) OR ([RefreshTokenHash] IS NOT NULL AND [RefreshTokenExpiresAt] IS NOT NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Suppliers_Email_NotEmpty",
                table: "Suppliers",
                sql: "[Email] IS NULL OR LEN([Email]) > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_StockMovements_Note_NotEmpty",
                table: "StockMovements",
                sql: "[Note] IS NULL OR LEN([Note]) > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PurchaseOrders_ReceivedAt_NotBeforeSentAt",
                table: "PurchaseOrders",
                sql: "[ReceivedAt] IS NULL OR [SentAt] IS NULL OR [ReceivedAt] >= [SentAt]");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PurchaseOrders_ReceivedAt_RequiresSentAt",
                table: "PurchaseOrders",
                sql: "[ReceivedAt] IS NULL OR [SentAt] IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Customers_Email_NotEmpty",
                table: "Customers",
                sql: "[Email] IS NULL OR LEN([Email]) > 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Warehouses_Address_NotEmpty",
                table: "Warehouses");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Users_RefreshToken_Pair",
                table: "Users");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Suppliers_Email_NotEmpty",
                table: "Suppliers");

            migrationBuilder.DropCheckConstraint(
                name: "CK_StockMovements_Note_NotEmpty",
                table: "StockMovements");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PurchaseOrders_ReceivedAt_NotBeforeSentAt",
                table: "PurchaseOrders");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PurchaseOrders_ReceivedAt_RequiresSentAt",
                table: "PurchaseOrders");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Customers_Email_NotEmpty",
                table: "Customers");
        }
    }
}
