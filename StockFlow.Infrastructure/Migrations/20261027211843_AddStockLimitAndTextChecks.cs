using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStockLimitAndTextChecks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Stocks_Quantity_NonNegative",
                table: "Stocks");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Warehouses_Name_NotEmpty",
                table: "Warehouses",
                sql: "LEN([Name]) > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Users_Email_NotEmpty",
                table: "Users",
                sql: "LEN([Email]) > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Users_EmployeeCode_NotEmpty",
                table: "Users",
                sql: "LEN([EmployeeCode]) > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Users_FullName_NotEmpty",
                table: "Users",
                sql: "LEN([FullName]) > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Users_PhoneNumber_Format",
                table: "Users",
                sql: "[PhoneNumber] IS NULL OR [PhoneNumber] LIKE '[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]'");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Suppliers_Name_NotEmpty",
                table: "Suppliers",
                sql: "LEN([Name]) > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Suppliers_PhoneNumber_Format",
                table: "Suppliers",
                sql: "[PhoneNumber] IS NULL OR [PhoneNumber] LIKE '[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]'");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Stocks_Quantity_Range",
                table: "Stocks",
                sql: "[Quantity] >= 0 AND [Quantity] <= 1000000000");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Products_Name_NotEmpty",
                table: "Products",
                sql: "LEN([Name]) > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Products_SKU_NotEmpty",
                table: "Products",
                sql: "LEN([SKU]) > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Customers_FullName_NotEmpty",
                table: "Customers",
                sql: "LEN([FullName]) > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Customers_PhoneNumber_Format",
                table: "Customers",
                sql: "[PhoneNumber] IS NULL OR [PhoneNumber] LIKE '[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]'");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Categories_Name_NotEmpty",
                table: "Categories",
                sql: "LEN([Name]) > 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Warehouses_Name_NotEmpty",
                table: "Warehouses");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Users_Email_NotEmpty",
                table: "Users");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Users_EmployeeCode_NotEmpty",
                table: "Users");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Users_FullName_NotEmpty",
                table: "Users");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Users_PhoneNumber_Format",
                table: "Users");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Suppliers_Name_NotEmpty",
                table: "Suppliers");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Suppliers_PhoneNumber_Format",
                table: "Suppliers");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Stocks_Quantity_Range",
                table: "Stocks");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Products_Name_NotEmpty",
                table: "Products");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Products_SKU_NotEmpty",
                table: "Products");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Customers_FullName_NotEmpty",
                table: "Customers");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Customers_PhoneNumber_Format",
                table: "Customers");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Categories_Name_NotEmpty",
                table: "Categories");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Stocks_Quantity_NonNegative",
                table: "Stocks",
                sql: "[Quantity] >= 0");
        }
    }
}
