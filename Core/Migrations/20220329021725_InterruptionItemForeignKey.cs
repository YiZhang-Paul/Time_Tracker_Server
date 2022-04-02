using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Core.Migrations
{
    public partial class InterruptionItemForeignKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "InterruptionItem",
                type: "bigint",
                nullable: false,
                defaultValue: 1L);

            migrationBuilder.CreateIndex(
                name: "IX_InterruptionItem_UserId",
                table: "InterruptionItem",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_InterruptionItem_UserProfile_UserId",
                table: "InterruptionItem",
                column: "UserId",
                principalTable: "UserProfile",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InterruptionItem_UserProfile_UserId",
                table: "InterruptionItem");

            migrationBuilder.DropIndex(
                name: "IX_InterruptionItem_UserId",
                table: "InterruptionItem");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "InterruptionItem");
        }
    }
}
