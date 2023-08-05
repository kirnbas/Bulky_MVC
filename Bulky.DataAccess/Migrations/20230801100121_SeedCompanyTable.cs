using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BulkyBook.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class SeedCompanyTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Companies",
                columns: new[] { "Id", "City", "Name", "PhoneNumber", "PostalCode", "State", "StreetAddress" },
                values: new object[,]
                {
                    { 1, "ZXCZXCZXC", "ZXC", "ZXCZXCZXCZXCZXCZXC", "ZXCZXCZXCZXCZXC", "ZXCZXC", "ZXCZXCZXCZXC" },
                    { 2, "ZXCZXCZXC", "NextCompany", "ZXCZXCZXCZXCZXCZXC", "ZXCZXCZXCZXCZXC", "ZXCZXC", "ZXCZXCZXCZXC" },
                    { 3, "ZXCZXCZXC", "TrendCo", "ZXCZXCZXCZXCZXCZXC", "ZXCZXCZXCZXCZXC", "ZXCZXC", "ZXCZXCZXCZXC" },
                    { 4, "ZXCZXCZXC", "CoffeeCo", "ZXCZXCZXCZXCZXCZXC", "ZXCZXCZXCZXCZXC", "ZXCZXC", "ZXCZXCZXCZXC" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: 4);
        }
    }
}
