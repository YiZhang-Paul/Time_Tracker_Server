using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Core.Migrations
{
    public partial class ChecklistEntry : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InterruptionChecklistEntry",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Description = table.Column<string>(maxLength: 100, nullable: false),
                    IsCompleted = table.Column<bool>(nullable: false),
                    ParentId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterruptionChecklistEntry", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InterruptionChecklistEntry_InterruptionItem_ParentId",
                        column: x => x.ParentId,
                        principalTable: "InterruptionItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaskChecklistEntry",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Description = table.Column<string>(maxLength: 100, nullable: false),
                    IsCompleted = table.Column<bool>(nullable: false),
                    ParentId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskChecklistEntry", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskChecklistEntry_TaskItem_ParentId",
                        column: x => x.ParentId,
                        principalTable: "TaskItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InterruptionChecklistEntry_ParentId",
                table: "InterruptionChecklistEntry",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskChecklistEntry_ParentId",
                table: "TaskChecklistEntry",
                column: "ParentId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InterruptionChecklistEntry");

            migrationBuilder.DropTable(
                name: "TaskChecklistEntry");
        }
    }
}
