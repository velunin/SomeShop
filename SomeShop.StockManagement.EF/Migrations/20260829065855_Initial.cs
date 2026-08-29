using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SomeShop.StockManagement.EF.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "stock_management");

            migrationBuilder.CreateTable(
                name: "reservations",
                schema: "stock_management",
                columns: table => new
                {
                    orderid = table.Column<Guid>(name: "order_id", type: "uuid", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    failreason = table.Column<string>(name: "fail_reason", type: "text", nullable: false),
                    createdat = table.Column<DateTimeOffset>(name: "created_at", type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_reservations", x => x.orderid);
                });

            migrationBuilder.CreateTable(
                name: "stock_items",
                schema: "stock_management",
                columns: table => new
                {
                    productid = table.Column<Guid>(name: "product_id", type: "uuid", nullable: false),
                    available = table.Column<long>(type: "bigint", nullable: false),
                    reserved = table.Column<long>(type: "bigint", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_stock_items", x => x.productid);
                });

            migrationBuilder.CreateTable(
                name: "reservation_items",
                schema: "stock_management",
                columns: table => new
                {
                    orderid = table.Column<Guid>(name: "order_id", type: "uuid", nullable: false),
                    productid = table.Column<Guid>(name: "product_id", type: "uuid", nullable: false),
                    quantity = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_reservation_items", x => new { x.orderid, x.productid });
                    table.ForeignKey(
                        name: "fk_reservation_items_reservations_order_id",
                        column: x => x.orderid,
                        principalSchema: "stock_management",
                        principalTable: "reservations",
                        principalColumn: "order_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql(@"
INSERT INTO stock_management.stock_items (product_id, available, reserved)
VALUES ('89572dc1-2fb1-488b-83db-1059c63cef37', 100, 0),
       ('9560242f-eb23-414b-9602-d5248f700b3c', 100, 0),
       ('d18853b6-ae63-4a07-807d-6fcaaa0bbb41', 100, 0);
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "reservation_items",
                schema: "stock_management");

            migrationBuilder.DropTable(
                name: "stock_items",
                schema: "stock_management");

            migrationBuilder.DropTable(
                name: "reservations",
                schema: "stock_management");
        }
    }
}
