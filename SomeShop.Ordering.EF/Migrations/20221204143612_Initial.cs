using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SomeShop.Ordering.EF.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "ordering");

            migrationBuilder.CreateTable(
                name: "carts",
                schema: "ordering",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    totalproductspositions = table.Column<long>(name: "total_products_positions", type: "bigint", nullable: false),
                    totalsumamount = table.Column<decimal>(name: "total_sum_amount", type: "numeric", nullable: false),
                    totalsumcurrency = table.Column<string>(name: "total_sum_currency", type: "varchar(3)", nullable: false),
                    createdat = table.Column<DateTimeOffset>(name: "created_at", type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_carts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "orders",
                schema: "ordering",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cartid = table.Column<Guid>(name: "cart_id", type: "uuid", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    failreason = table.Column<string>(name: "fail_reason", type: "text", nullable: false),
                    reservationstatus = table.Column<string>(name: "reservation_status", type: "text", nullable: false),
                    totalsumamount = table.Column<decimal>(name: "total_sum_amount", type: "numeric", nullable: false),
                    totalsumcurrency = table.Column<string>(name: "total_sum_currency", type: "varchar(3)", nullable: false),
                    createdat = table.Column<DateTimeOffset>(name: "created_at", type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_orders", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "cart_items",
                schema: "ordering",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cartid = table.Column<Guid>(name: "cart_id", type: "uuid", nullable: false),
                    productid = table.Column<Guid>(name: "product_id", type: "uuid", nullable: false),
                    priceamount = table.Column<decimal>(name: "price_amount", type: "numeric", nullable: false),
                    pricecurrency = table.Column<string>(name: "price_currency", type: "varchar(3)", nullable: false),
                    quantity = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cart_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_cart_items_carts_cart_temp_id",
                        column: x => x.cartid,
                        principalSchema: "ordering",
                        principalTable: "carts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "order_items",
                schema: "ordering",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    orderid = table.Column<Guid>(name: "order_id", type: "uuid", nullable: false),
                    productid = table.Column<Guid>(name: "product_id", type: "uuid", nullable: false),
                    quantity = table.Column<long>(type: "bigint", nullable: false),
                    priceamount = table.Column<decimal>(name: "price_amount", type: "numeric", nullable: false),
                    pricecurrency = table.Column<string>(name: "price_currency", type: "varchar(3)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_order_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_order_items_orders_order_temp_id",
                        column: x => x.orderid,
                        principalSchema: "ordering",
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_cart_items_cart_id",
                schema: "ordering",
                table: "cart_items",
                column: "cart_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_items_order_id",
                schema: "ordering",
                table: "order_items",
                column: "order_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cart_items",
                schema: "ordering");

            migrationBuilder.DropTable(
                name: "order_items",
                schema: "ordering");

            migrationBuilder.DropTable(
                name: "carts",
                schema: "ordering");

            migrationBuilder.DropTable(
                name: "orders",
                schema: "ordering");
        }
    }
}
