using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyEcommerce.DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOrderColumnSissionNameToSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SissionId",
                table: "OrderHeaders",
                newName: "SessionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SessionId",
                table: "OrderHeaders",
                newName: "SissionId");
        }
    }
}
