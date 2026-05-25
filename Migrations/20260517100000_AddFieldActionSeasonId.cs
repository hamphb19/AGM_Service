using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AGM_API.Migrations
{
    /// <inheritdoc />
    public partial class AddFieldActionSeasonId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "SeasonId",
                table: "FieldAction",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FieldAction_SeasonId",
                table: "FieldAction",
                column: "SeasonId");

            migrationBuilder.AddForeignKey(
                name: "FK_FieldAction_Season_SeasonId",
                table: "FieldAction",
                column: "SeasonId",
                principalTable: "Season",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FieldAction_Season_SeasonId",
                table: "FieldAction");

            migrationBuilder.DropIndex(
                name: "IX_FieldAction_SeasonId",
                table: "FieldAction");

            migrationBuilder.DropColumn(
                name: "SeasonId",
                table: "FieldAction");
        }
    }
}
