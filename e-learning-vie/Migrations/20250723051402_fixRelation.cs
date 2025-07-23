using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace e_learning_vie.Migrations
{
    /// <inheritdoc />
    public partial class fixRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK__StudGrades__Stude__JE26KNE7",
                table: "StudentScores");

            migrationBuilder.DropForeignKey(
                name: "FK__StudGrades__SubGra__9DM39NE7",
                table: "StudentScores");

            migrationBuilder.RenameColumn(
                name: "StudentID",
                table: "StudentScores",
                newName: "StudentSubjectID");

            migrationBuilder.RenameIndex(
                name: "IX_StudentScores_StudentID",
                table: "StudentScores",
                newName: "IX_StudentScores_StudentSubjectID");

            migrationBuilder.CreateTable(
                name: "StudentSubjects",
                columns: table => new
                {
                    StudentSubjectId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ScoreDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsProgress = table.Column<bool>(type: "bit", nullable: true),
                    StudentID = table.Column<int>(type: "int", nullable: false),
                    SubjectID = table.Column<int>(type: "int", nullable: false),
                    AcademicYearID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__StuSubje__83JUDNEKEN88D7", x => x.StudentSubjectId);
                    table.ForeignKey(
                        name: "FK__StudSubje__Acade__9JEHN7HE",
                        column: x => x.AcademicYearID,
                        principalTable: "AcademicYears",
                        principalColumn: "AcademicYearID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK__StudSubje__Stude__DI9JM8JE",
                        column: x => x.StudentID,
                        principalTable: "Students",
                        principalColumn: "StudentID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK__StudSubje__Subjec__JE2JEND9",
                        column: x => x.SubjectID,
                        principalTable: "Subjects",
                        principalColumn: "SubjectID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StudentSubjects_AcademicYearID",
                table: "StudentSubjects",
                column: "AcademicYearID");

            migrationBuilder.CreateIndex(
                name: "IX_StudentSubjects_StudentID",
                table: "StudentSubjects",
                column: "StudentID");

            migrationBuilder.CreateIndex(
                name: "IX_StudentSubjects_SubjectID",
                table: "StudentSubjects",
                column: "SubjectID");

            migrationBuilder.AddForeignKey(
                name: "FK__StudGrades__StuSub__KEIDM83J",
                table: "StudentScores",
                column: "StudentSubjectID",
                principalTable: "StudentSubjects",
                principalColumn: "StudentSubjectId");

            migrationBuilder.AddForeignKey(
                name: "FK__StudGrades__SubGra__9DM39NE7",
                table: "StudentScores",
                column: "SubjectGradeID",
                principalTable: "SubjectGrades",
                principalColumn: "SubjectGradeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK__StudGrades__StuSub__KEIDM83J",
                table: "StudentScores");

            migrationBuilder.DropForeignKey(
                name: "FK__StudGrades__SubGra__9DM39NE7",
                table: "StudentScores");

            migrationBuilder.DropTable(
                name: "StudentSubjects");

            migrationBuilder.RenameColumn(
                name: "StudentSubjectID",
                table: "StudentScores",
                newName: "StudentID");

            migrationBuilder.RenameIndex(
                name: "IX_StudentScores_StudentSubjectID",
                table: "StudentScores",
                newName: "IX_StudentScores_StudentID");

            migrationBuilder.AddForeignKey(
                name: "FK__StudGrades__Stude__JE26KNE7",
                table: "StudentScores",
                column: "StudentID",
                principalTable: "Students",
                principalColumn: "StudentID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK__StudGrades__SubGra__9DM39NE7",
                table: "StudentScores",
                column: "SubjectGradeID",
                principalTable: "SubjectGrades",
                principalColumn: "SubjectGradeId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
