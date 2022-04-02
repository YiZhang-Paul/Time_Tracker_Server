using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Core.Migrations
{
    public partial class EventsForeignKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "EventPrompt",
                type: "bigint",
                nullable: false,
                defaultValue: 1L);

            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "EventHistory",
                type: "bigint",
                nullable: false,
                defaultValue: 1L);

            migrationBuilder.CreateIndex(
                name: "IX_EventPrompt_UserId",
                table: "EventPrompt",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_EventHistory_UserId",
                table: "EventHistory",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_EventHistory_UserProfile_UserId",
                table: "EventHistory",
                column: "UserId",
                principalTable: "UserProfile",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EventPrompt_UserProfile_UserId",
                table: "EventPrompt",
                column: "UserId",
                principalTable: "UserProfile",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventHistory_UserProfile_UserId",
                table: "EventHistory");

            migrationBuilder.DropForeignKey(
                name: "FK_EventPrompt_UserProfile_UserId",
                table: "EventPrompt");

            migrationBuilder.DropIndex(
                name: "IX_EventPrompt_UserId",
                table: "EventPrompt");

            migrationBuilder.DropIndex(
                name: "IX_EventHistory_UserId",
                table: "EventHistory");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "EventPrompt");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "EventHistory");
        }
    }
}
