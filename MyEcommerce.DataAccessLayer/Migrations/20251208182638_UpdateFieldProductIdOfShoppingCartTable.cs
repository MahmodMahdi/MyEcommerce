using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyEcommerce.DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFieldProductIdOfShoppingCartTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShoppingCarts_Products_ProdcutId",
                table: "ShoppingCarts");

            migrationBuilder.RenameColumn(
                name: "ProdcutId",
                table: "ShoppingCarts",
                newName: "ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_ShoppingCarts_ProdcutId",
                table: "ShoppingCarts",
                newName: "IX_ShoppingCarts_ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_ShoppingCarts_Products_ProductId",
                table: "ShoppingCarts",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShoppingCarts_Products_ProductId",
                table: "ShoppingCarts");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "ShoppingCarts",
                newName: "ProdcutId");

            migrationBuilder.RenameIndex(
                name: "IX_ShoppingCarts_ProductId",
                table: "ShoppingCarts",
                newName: "IX_ShoppingCarts_ProdcutId");

            migrationBuilder.AddForeignKey(
                name: "FK_ShoppingCarts_Products_ProdcutId",
                table: "ShoppingCarts",
                column: "ProdcutId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
