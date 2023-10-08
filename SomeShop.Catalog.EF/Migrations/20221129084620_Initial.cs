using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SomeShop.Catalog.EF.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "catalog");

            migrationBuilder.CreateTable(
                name: "products",
                schema: "catalog",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "varchar(50)", nullable: false),
                    priceamount = table.Column<decimal>(name: "price_amount", type: "numeric", nullable: false),
                    pricecurrency = table.Column<string>(name: "price_currency", type: "varchar(3)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_products", x => x.id);
                });
            
            migrationBuilder.Sql(@"
INSERT INTO catalog.products (id, name, price_amount, price_currency) 
VALUES ('89572dc1-2fb1-488b-83db-1059c63cef37', 'Amazing product', 599.70, 'RUB'),
       ('9560242f-eb23-414b-9602-d5248f700b3c', 'Gorgeous product', 988.50, 'RUB'),
       ('d18853b6-ae63-4a07-807d-6fcaaa0bbb41', 'Incredible product', 1460.00, 'RUB');
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "products",
                schema: "catalog");
        }
    }
}
