using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AGM_API.Migrations
{
    /// <inheritdoc />
    public partial class TaskMultiField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Task_Field_FieldId",
                table: "Task");

            migrationBuilder.DropIndex(
                name: "IX_Task_FieldId",
                table: "Task");

            migrationBuilder.DropColumn(
                name: "FieldId",
                table: "Task");

            migrationBuilder.CreateTable(
                name: "TaskField",
                columns: table => new
                {
                    TaskId = table.Column<long>(type: "bigint", nullable: false),
                    FieldId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskField", x => new { x.TaskId, x.FieldId });
                    table.ForeignKey(
                        name: "FK_TaskField_Field_FieldId",
                        column: x => x.FieldId,
                        principalTable: "Field",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaskField_Task_TaskId",
                        column: x => x.TaskId,
                        principalTable: "Task",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaskField_FieldId",
                table: "TaskField",
                column: "FieldId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaskField");

            migrationBuilder.AddColumn<long>(
                name: "FieldId",
                table: "Task",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Task_FieldId",
                table: "Task",
                column: "FieldId");

            migrationBuilder.AddForeignKey(
                name: "FK_Task_Field_FieldId",
                table: "Task",
                column: "FieldId",
                principalTable: "Field",
                principalColumn: "Id");
        }
    }
}
