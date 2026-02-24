using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoginForm.Migrations
{
    /// <inheritdoc />
    public partial class emailveriy_col : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EmailVerifed",
                table: "StudentsRegistration",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailVerifed",
                table: "StudentsRegistration");
        }
    }
}
