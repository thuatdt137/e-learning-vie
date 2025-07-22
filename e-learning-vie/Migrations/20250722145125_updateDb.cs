using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace e_learning_vie.Migrations
{
    /// <inheritdoc />
    public partial class updateDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK__Classes__Academi__2E1BDC42",
                table: "Classes");

            migrationBuilder.DropForeignKey(
                name: "FK__Classes__Teacher__2F10007B",
                table: "Classes");

            migrationBuilder.DropForeignKey(
                name: "FK__Grades__Academic__3E52440B",
                table: "Grades");

            migrationBuilder.DropForeignKey(
                name: "FK__Grades__StudentI__3C69FB99",
                table: "Grades");

            migrationBuilder.DropForeignKey(
                name: "FK__Grades__SubjectI__3D5E1FD2",
                table: "Grades");

            migrationBuilder.DropForeignKey(
                name: "FK__Students__ClassI__32E0915F",
                table: "Students");

            migrationBuilder.DropForeignKey(
                name: "FK__Students__School__33D4B598",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Grades_StudentID",
                table: "Grades");

            migrationBuilder.DropIndex(
                name: "IX_Grades_SubjectID",
                table: "Grades");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_StudentId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_TeacherId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DateEntered",
                table: "Grades");

            migrationBuilder.DropColumn(
                name: "Score",
                table: "Grades");

            migrationBuilder.DropColumn(
                name: "StudentID",
                table: "Grades");

            migrationBuilder.DropColumn(
                name: "SubjectID",
                table: "Grades");

            migrationBuilder.RenameColumn(
                name: "SchoolID",
                table: "Students",
                newName: "SchoolId");

            migrationBuilder.RenameColumn(
                name: "ClassID",
                table: "Students",
                newName: "ClassId");

            migrationBuilder.RenameIndex(
                name: "IX_Students_SchoolID",
                table: "Students",
                newName: "IX_Students_SchoolId");

            migrationBuilder.RenameIndex(
                name: "IX_Students_ClassID",
                table: "Students",
                newName: "IX_Students_ClassId");

            migrationBuilder.RenameColumn(
                name: "AcademicYearID",
                table: "Grades",
                newName: "AcademicYearId");

            migrationBuilder.RenameIndex(
                name: "IX_Grades_AcademicYearID",
                table: "Grades",
                newName: "IX_Grades_AcademicYearId");

            migrationBuilder.RenameColumn(
                name: "TeacherID",
                table: "Classes",
                newName: "TeacherId");

            migrationBuilder.RenameColumn(
                name: "AcademicYearID",
                table: "Classes",
                newName: "AcademicYearId");

            migrationBuilder.RenameIndex(
                name: "IX_Classes_TeacherID",
                table: "Classes",
                newName: "IX_Classes_TeacherId");

            migrationBuilder.RenameIndex(
                name: "IX_Classes_AcademicYearID",
                table: "Classes",
                newName: "IX_Classes_AcademicYearId");

            migrationBuilder.AddColumn<string>(
                name: "IdentityCode",
                table: "Teachers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SubjectType",
                table: "Subjects",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "SchoolID",
                table: "StudentClassHistory",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TeacherID",
                table: "StudentClassHistory",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Grades",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SubjectGrades",
                columns: table => new
                {
                    SubjectGradeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubjectID = table.Column<int>(type: "int", nullable: false),
                    GradeID = table.Column<int>(type: "int", nullable: false),
                    Weight = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__SubGrades__54F87JD7G2E8537", x => x.SubjectGradeId);
                    table.ForeignKey(
                        name: "FK__SubGrades__Grade__JD76KNE7",
                        column: x => x.GradeID,
                        principalTable: "Grades",
                        principalColumn: "GradeID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK__SubGrades__Subje__8DJ7J7H4",
                        column: x => x.SubjectID,
                        principalTable: "Subjects",
                        principalColumn: "SubjectID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudentScores",
                columns: table => new
                {
                    StudentScoreId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Score = table.Column<double>(type: "float", nullable: true),
                    ScoreDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StudentID = table.Column<int>(type: "int", nullable: false),
                    SubjectGradeID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__StuScores__83JWN6H9HWN78537", x => x.StudentScoreId);
                    table.ForeignKey(
                        name: "FK__StudGrades__Stude__JE26KNE7",
                        column: x => x.StudentID,
                        principalTable: "Students",
                        principalColumn: "StudentID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK__StudGrades__SubGra__9DM39NE7",
                        column: x => x.SubjectGradeID,
                        principalTable: "SubjectGrades",
                        principalColumn: "SubjectGradeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StudentClassHistory_SchoolID",
                table: "StudentClassHistory",
                column: "SchoolID");

            migrationBuilder.CreateIndex(
                name: "IX_StudentClassHistory_TeacherID",
                table: "StudentClassHistory",
                column: "TeacherID");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_StudentId",
                table: "AspNetUsers",
                column: "StudentId",
                unique: true,
                filter: "[StudentId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_TeacherId",
                table: "AspNetUsers",
                column: "TeacherId",
                unique: true,
                filter: "[TeacherId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_StudentScores_StudentID",
                table: "StudentScores",
                column: "StudentID");

            migrationBuilder.CreateIndex(
                name: "IX_StudentScores_SubjectGradeID",
                table: "StudentScores",
                column: "SubjectGradeID");

            migrationBuilder.CreateIndex(
                name: "IX_SubjectGrades_GradeID",
                table: "SubjectGrades",
                column: "GradeID");

            migrationBuilder.CreateIndex(
                name: "IX_SubjectGrades_SubjectID",
                table: "SubjectGrades",
                column: "SubjectID");

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
                name: "FK__StudentCl__Schoo__581284D9",
                table: "StudentClassHistory",
                column: "SchoolID",
                principalTable: "Schools",
                principalColumn: "SchoolID");

            migrationBuilder.AddForeignKey(
                name: "FK__TeacherCl__Teach__571DF1D5",
                table: "StudentClassHistory",
                column: "TeacherID",
                principalTable: "Teachers",
                principalColumn: "TeacherID");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
                name: "FK__StudentCl__Schoo__581284D9",
                table: "StudentClassHistory");

            migrationBuilder.DropForeignKey(
                name: "FK__TeacherCl__Teach__571DF1D5",
                table: "StudentClassHistory");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_Classes_ClassId",
                table: "Students");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_Schools_SchoolId",
                table: "Students");

            migrationBuilder.DropTable(
                name: "StudentScores");

            migrationBuilder.DropTable(
                name: "SubjectGrades");

            migrationBuilder.DropIndex(
                name: "IX_StudentClassHistory_SchoolID",
                table: "StudentClassHistory");

            migrationBuilder.DropIndex(
                name: "IX_StudentClassHistory_TeacherID",
                table: "StudentClassHistory");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_StudentId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_TeacherId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "IdentityCode",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "SubjectType",
                table: "Subjects");

            migrationBuilder.DropColumn(
                name: "SchoolID",
                table: "StudentClassHistory");

            migrationBuilder.DropColumn(
                name: "TeacherID",
                table: "StudentClassHistory");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Grades");

            migrationBuilder.RenameColumn(
                name: "SchoolId",
                table: "Students",
                newName: "SchoolID");

            migrationBuilder.RenameColumn(
                name: "ClassId",
                table: "Students",
                newName: "ClassID");

            migrationBuilder.RenameIndex(
                name: "IX_Students_SchoolId",
                table: "Students",
                newName: "IX_Students_SchoolID");

            migrationBuilder.RenameIndex(
                name: "IX_Students_ClassId",
                table: "Students",
                newName: "IX_Students_ClassID");

            migrationBuilder.RenameColumn(
                name: "AcademicYearId",
                table: "Grades",
                newName: "AcademicYearID");

            migrationBuilder.RenameIndex(
                name: "IX_Grades_AcademicYearId",
                table: "Grades",
                newName: "IX_Grades_AcademicYearID");

            migrationBuilder.RenameColumn(
                name: "TeacherId",
                table: "Classes",
                newName: "TeacherID");

            migrationBuilder.RenameColumn(
                name: "AcademicYearId",
                table: "Classes",
                newName: "AcademicYearID");

            migrationBuilder.RenameIndex(
                name: "IX_Classes_TeacherId",
                table: "Classes",
                newName: "IX_Classes_TeacherID");

            migrationBuilder.RenameIndex(
                name: "IX_Classes_AcademicYearId",
                table: "Classes",
                newName: "IX_Classes_AcademicYearID");

            migrationBuilder.AddColumn<DateOnly>(
                name: "DateEntered",
                table: "Grades",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Score",
                table: "Grades",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StudentID",
                table: "Grades",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SubjectID",
                table: "Grades",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Grades_StudentID",
                table: "Grades",
                column: "StudentID");

            migrationBuilder.CreateIndex(
                name: "IX_Grades_SubjectID",
                table: "Grades",
                column: "SubjectID");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_StudentId",
                table: "AspNetUsers",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_TeacherId",
                table: "AspNetUsers",
                column: "TeacherId");

            migrationBuilder.AddForeignKey(
                name: "FK__Classes__Academi__2E1BDC42",
                table: "Classes",
                column: "AcademicYearID",
                principalTable: "AcademicYears",
                principalColumn: "AcademicYearID");

            migrationBuilder.AddForeignKey(
                name: "FK__Classes__Teacher__2F10007B",
                table: "Classes",
                column: "TeacherID",
                principalTable: "Teachers",
                principalColumn: "TeacherID");

            migrationBuilder.AddForeignKey(
                name: "FK__Grades__Academic__3E52440B",
                table: "Grades",
                column: "AcademicYearID",
                principalTable: "AcademicYears",
                principalColumn: "AcademicYearID");

            migrationBuilder.AddForeignKey(
                name: "FK__Grades__StudentI__3C69FB99",
                table: "Grades",
                column: "StudentID",
                principalTable: "Students",
                principalColumn: "StudentID");

            migrationBuilder.AddForeignKey(
                name: "FK__Grades__SubjectI__3D5E1FD2",
                table: "Grades",
                column: "SubjectID",
                principalTable: "Subjects",
                principalColumn: "SubjectID");

            migrationBuilder.AddForeignKey(
                name: "FK__Students__ClassI__32E0915F",
                table: "Students",
                column: "ClassID",
                principalTable: "Classes",
                principalColumn: "ClassID");

            migrationBuilder.AddForeignKey(
                name: "FK__Students__School__33D4B598",
                table: "Students",
                column: "SchoolID",
                principalTable: "Schools",
                principalColumn: "SchoolID");
        }
    }
}
