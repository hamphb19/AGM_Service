using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AGM_API.Migrations
{
    /// <inheritdoc />
    public partial class AddUserKeycloakSubject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "KeycloakSubject",
                table: "User",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_KeycloakSubject",
                table: "User",
                column: "KeycloakSubject",
                unique: true,
                filter: "\"KeycloakSubject\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_User_KeycloakSubject",
                table: "User");

            migrationBuilder.DropColumn(
                name: "KeycloakSubject",
                table: "User");
        }
    }
}
