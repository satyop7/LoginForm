using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoginForm.Migrations
{
    /// <inheritdoc />
    public partial class changesToDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Country",
                table: "StudentsRegistration",
                newName: "Pincode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Pincode",
                table: "StudentsRegistration",
                newName: "Country");
        }
    }
}
