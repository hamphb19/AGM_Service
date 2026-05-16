using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AGM_API.Migrations
{
    /// <inheritdoc />
    public partial class AddFarmIdToField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "FarmId",
                table: "Field",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_Field_FarmId",
                table: "Field",
                column: "FarmId");

            migrationBuilder.AddForeignKey(
                name: "FK_Field_Farm_FarmId",
                table: "Field",
                column: "FarmId",
                principalTable: "Farm",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Field_Farm_FarmId",
                table: "Field");

            migrationBuilder.DropIndex(
                name: "IX_Field_FarmId",
                table: "Field");

            migrationBuilder.DropColumn(
                name: "FarmId",
                table: "Field");
        }
    }
}
