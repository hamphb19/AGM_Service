using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AGM_API.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTaskShortName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShortName",
                table: "Task");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ShortName",
                table: "Task",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
