using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Core.Migrations
{
    public partial class UseFluentApi : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InterruptionChecklistEntry_InterruptionItem_ParentId",
                table: "InterruptionChecklistEntry");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskChecklistEntry_TaskItem_ParentId",
                table: "TaskChecklistEntry");

            migrationBuilder.RenameColumn(
                name: "ParentId",
                table: "TaskChecklistEntry",
                newName: "TaskItemId");

            migrationBuilder.RenameIndex(
                name: "IX_TaskChecklistEntry_ParentId",
                table: "TaskChecklistEntry",
                newName: "IX_TaskChecklistEntry_TaskItemId");

            migrationBuilder.RenameColumn(
                name: "ParentId",
                table: "InterruptionChecklistEntry",
                newName: "InterruptionItemId");

            migrationBuilder.RenameIndex(
                name: "IX_InterruptionChecklistEntry_ParentId",
                table: "InterruptionChecklistEntry",
                newName: "IX_InterruptionChecklistEntry_InterruptionItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_InterruptionChecklistEntry_InterruptionItem_InterruptionIte~",
                table: "InterruptionChecklistEntry",
                column: "InterruptionItemId",
                principalTable: "InterruptionItem",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskChecklistEntry_TaskItem_TaskItemId",
                table: "TaskChecklistEntry",
                column: "TaskItemId",
                principalTable: "TaskItem",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InterruptionChecklistEntry_InterruptionItem_InterruptionIte~",
                table: "InterruptionChecklistEntry");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskChecklistEntry_TaskItem_TaskItemId",
                table: "TaskChecklistEntry");

            migrationBuilder.RenameColumn(
                name: "TaskItemId",
                table: "TaskChecklistEntry",
                newName: "ParentId");

            migrationBuilder.RenameIndex(
                name: "IX_TaskChecklistEntry_TaskItemId",
                table: "TaskChecklistEntry",
                newName: "IX_TaskChecklistEntry_ParentId");

            migrationBuilder.RenameColumn(
                name: "InterruptionItemId",
                table: "InterruptionChecklistEntry",
                newName: "ParentId");

            migrationBuilder.RenameIndex(
                name: "IX_InterruptionChecklistEntry_InterruptionItemId",
                table: "InterruptionChecklistEntry",
                newName: "IX_InterruptionChecklistEntry_ParentId");

            migrationBuilder.AddForeignKey(
                name: "FK_InterruptionChecklistEntry_InterruptionItem_ParentId",
                table: "InterruptionChecklistEntry",
                column: "ParentId",
                principalTable: "InterruptionItem",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskChecklistEntry_TaskItem_ParentId",
                table: "TaskChecklistEntry",
                column: "ParentId",
                principalTable: "TaskItem",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
