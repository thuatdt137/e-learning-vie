using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace e_learning_vie.Migrations
{
    /// <inheritdoc />
    public partial class idcodeAndGradeType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IdentityCode",
                table: "Students",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "GradeType",
                table: "Grades",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IdentityCode",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "GradeType",
                table: "Grades");
        }
    }
}
