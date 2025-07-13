using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace e_learning_vie.Migrations
{
    /// <inheritdoc />
    public partial class classNameHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClassName",
                table: "StudentClassHistory",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SchoolName",
                table: "StudentClassHistory",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClassName",
                table: "StudentClassHistory");

            migrationBuilder.DropColumn(
                name: "SchoolName",
                table: "StudentClassHistory");
        }
    }
}
