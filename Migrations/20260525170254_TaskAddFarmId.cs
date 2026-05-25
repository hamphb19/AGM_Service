using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AGM_API.Migrations
{
    /// <inheritdoc />
    public partial class TaskAddFarmId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add as nullable first so existing rows don't violate NOT NULL
            migrationBuilder.AddColumn<long>(
                name: "FarmId",
                table: "Task",
                type: "bigint",
                nullable: true);

            // Fill FarmId from Season → Farm for existing tasks
            migrationBuilder.Sql(@"
                UPDATE ""Task"" t
                SET ""FarmId"" = s.""FarmId""
                FROM ""Season"" s
                WHERE t.""SeasonId"" = s.""Id"";
            ");

            // Remove tasks that couldn't be associated with any farm
            migrationBuilder.Sql(@"DELETE FROM ""Task"" WHERE ""FarmId"" IS NULL;");

            // Now make it NOT NULL
            migrationBuilder.AlterColumn<long>(
                name: "FarmId",
                table: "Task",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Task_FarmId",
                table: "Task",
                column: "FarmId");

            migrationBuilder.AddForeignKey(
                name: "FK_Task_Farm_FarmId",
                table: "Task",
                column: "FarmId",
                principalTable: "Farm",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Task_Farm_FarmId",
                table: "Task");

            migrationBuilder.DropIndex(
                name: "IX_Task_FarmId",
                table: "Task");

            migrationBuilder.DropColumn(
                name: "FarmId",
                table: "Task");
        }
    }
}
