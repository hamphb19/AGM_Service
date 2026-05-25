using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AGM_API.Migrations
{
    public partial class AddUserCode : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserCode",
                table: "User",
                type: "character varying(8)",
                maxLength: 8,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_UserCode",
                table: "User",
                column: "UserCode",
                unique: true,
                filter: "\"UserCode\" IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_User_UserCode",
                table: "User");

            migrationBuilder.DropColumn(
                name: "UserCode",
                table: "User");
        }
    }
}
