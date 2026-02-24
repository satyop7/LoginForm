using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoginForm.Migrations
{
    /// <inheritdoc />
    public partial class otpcolumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OTP",
                table: "StudentsRegistration",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OTP",
                table: "StudentsRegistration");
        }
    }
}
