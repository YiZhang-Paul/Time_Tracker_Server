using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Core.Migrations
{
    public partial class TimeSessionOptions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TimeSessionOptions_BreakSessionDuration",
                table: "UserProfile",
                type: "integer",
                nullable: false,
                defaultValue: 10 * 60 * 1000);

            migrationBuilder.AddColumn<int>(
                name: "TimeSessionOptions_DailyWorkDuration",
                table: "UserProfile",
                type: "integer",
                nullable: false,
                defaultValue: 8 * 60 * 60 * 1000);

            migrationBuilder.AddColumn<int>(
                name: "TimeSessionOptions_WorkSessionDuration",
                table: "UserProfile",
                type: "integer",
                nullable: false,
                defaultValue: 50 * 60 * 1000);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TimeSessionOptions_BreakSessionDuration",
                table: "UserProfile");

            migrationBuilder.DropColumn(
                name: "TimeSessionOptions_DailyWorkDuration",
                table: "UserProfile");

            migrationBuilder.DropColumn(
                name: "TimeSessionOptions_WorkSessionDuration",
                table: "UserProfile");
        }
    }
}
