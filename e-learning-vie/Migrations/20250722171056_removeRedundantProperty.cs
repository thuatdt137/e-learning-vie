using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace e_learning_vie.Migrations
{
    /// <inheritdoc />
    public partial class removeRedundantProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Classes_AcademicYears_AcademicYearId",
                table: "Classes");

            migrationBuilder.DropForeignKey(
                name: "FK_Classes_Teachers_TeacherId",
                table: "Classes");

            migrationBuilder.DropForeignKey(
                name: "FK_Grades_AcademicYears_AcademicYearId",
                table: "Grades");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_Classes_ClassId",
                table: "Students");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_Schools_SchoolId",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Students_ClassId",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Students_SchoolId",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Grades_AcademicYearId",
                table: "Grades");

            migrationBuilder.DropIndex(
                name: "IX_Classes_AcademicYearId",
                table: "Classes");

            migrationBuilder.DropIndex(
                name: "IX_Classes_TeacherId",
                table: "Classes");

            migrationBuilder.DropColumn(
                name: "ClassId",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "SchoolId",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "AcademicYearId",
                table: "Grades");

            migrationBuilder.DropColumn(
                name: "AcademicYearId",
                table: "Classes");

            migrationBuilder.DropColumn(
                name: "TeacherId",
                table: "Classes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClassId",
                table: "Students",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SchoolId",
                table: "Students",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AcademicYearId",
                table: "Grades",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AcademicYearId",
                table: "Classes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TeacherId",
                table: "Classes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Students_ClassId",
                table: "Students",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_Students_SchoolId",
                table: "Students",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_Grades_AcademicYearId",
                table: "Grades",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_Classes_AcademicYearId",
                table: "Classes",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_Classes_TeacherId",
                table: "Classes",
                column: "TeacherId");

            migrationBuilder.AddForeignKey(
                name: "FK_Classes_AcademicYears_AcademicYearId",
                table: "Classes",
                column: "AcademicYearId",
                principalTable: "AcademicYears",
                principalColumn: "AcademicYearID");

            migrationBuilder.AddForeignKey(
                name: "FK_Classes_Teachers_TeacherId",
                table: "Classes",
                column: "TeacherId",
                principalTable: "Teachers",
                principalColumn: "TeacherID");

            migrationBuilder.AddForeignKey(
                name: "FK_Grades_AcademicYears_AcademicYearId",
                table: "Grades",
                column: "AcademicYearId",
                principalTable: "AcademicYears",
                principalColumn: "AcademicYearID");

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Classes_ClassId",
                table: "Students",
                column: "ClassId",
                principalTable: "Classes",
                principalColumn: "ClassID");

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Schools_SchoolId",
                table: "Students",
                column: "SchoolId",
                principalTable: "Schools",
                principalColumn: "SchoolID");
        }
    }
}
