using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AGM_API.Migrations
{
    /// <inheritdoc />
    public partial class AddFieldActionCrop : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "CropId",
                table: "FieldAction",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FieldAction_CropId",
                table: "FieldAction",
                column: "CropId");

            migrationBuilder.AddForeignKey(
                name: "FK_FieldAction_Crop_CropId",
                table: "FieldAction",
                column: "CropId",
                principalTable: "Crop",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FieldAction_Crop_CropId",
                table: "FieldAction");

            migrationBuilder.DropIndex(
                name: "IX_FieldAction_CropId",
                table: "FieldAction");

            migrationBuilder.DropColumn(
                name: "CropId",
                table: "FieldAction");
        }
    }
}
