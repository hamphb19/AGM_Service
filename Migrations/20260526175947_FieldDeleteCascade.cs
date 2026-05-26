using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AGM_API.Migrations
{
    /// <inheritdoc />
    public partial class FieldDeleteCascade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SeasonField_Field_field_Id",
                table: "SeasonField");

            migrationBuilder.AddForeignKey(
                name: "FK_SeasonField_Field_field_Id",
                table: "SeasonField",
                column: "field_Id",
                principalTable: "Field",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SeasonField_Field_field_Id",
                table: "SeasonField");

            migrationBuilder.AddForeignKey(
                name: "FK_SeasonField_Field_field_Id",
                table: "SeasonField",
                column: "field_Id",
                principalTable: "Field",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
