using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoginForm.Migrations
{
    /// <inheritdoc />
    public partial class UpdateExamLogic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCompleted",
                table: "Exam");

            migrationBuilder.RenameColumn(
                name: "StudentEmail",
                table: "Exam",
                newName: "Program");

            migrationBuilder.AddColumn<int>(
                name: "ExamId",
                table: "Result",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExamId",
                table: "Result");

            migrationBuilder.RenameColumn(
                name: "Program",
                table: "Exam",
                newName: "StudentEmail");

            migrationBuilder.AddColumn<bool>(
                name: "IsCompleted",
                table: "Exam",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
