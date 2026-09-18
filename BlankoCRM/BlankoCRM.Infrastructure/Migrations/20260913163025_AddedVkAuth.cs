using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlankoCRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedVkAuth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "VkUserId",
                table: "AspNetUsers",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_VkUserId",
                table: "AspNetUsers",
                column: "VkUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_VkUserId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "VkUserId",
                table: "AspNetUsers");
        }
    }
}
