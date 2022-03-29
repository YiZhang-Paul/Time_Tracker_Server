using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Core.Migrations
{
    public partial class TaskItemForeignKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "TaskItem",
                type: "bigint",
                nullable: false,
                defaultValue: 1L);

            migrationBuilder.CreateIndex(
                name: "IX_TaskItem_UserId",
                table: "TaskItem",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskItem_UserProfile_UserId",
                table: "TaskItem",
                column: "UserId",
                principalTable: "UserProfile",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskItem_UserProfile_UserId",
                table: "TaskItem");

            migrationBuilder.DropIndex(
                name: "IX_TaskItem_UserId",
                table: "TaskItem");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "TaskItem");
        }
    }
}
