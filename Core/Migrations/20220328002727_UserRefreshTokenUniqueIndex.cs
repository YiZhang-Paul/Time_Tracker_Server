using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Core.Migrations
{
    public partial class UserRefreshTokenUniqueIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "AK_UserRefreshToken_RefreshToken",
                table: "UserRefreshToken");

            migrationBuilder.CreateIndex(
                name: "IX_UserRefreshToken_RefreshToken",
                table: "UserRefreshToken",
                column: "RefreshToken",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserRefreshToken_RefreshToken",
                table: "UserRefreshToken");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_UserRefreshToken_RefreshToken",
                table: "UserRefreshToken",
                column: "RefreshToken");
        }
    }
}
