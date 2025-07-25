using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace e_learning_vie.Migrations
{
    /// <inheritdoc />
    public partial class subjectscore2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK__StudentScore__Subject",
                table: "StudentScores");

            migrationBuilder.RenameColumn(
                name: "SubjectID",
                table: "StudentScores",
                newName: "SubjectScoreID");

            migrationBuilder.RenameIndex(
                name: "IX_StudentScores_SubjectID",
                table: "StudentScores",
                newName: "IX_StudentScores_SubjectScoreID");

            migrationBuilder.AddForeignKey(
                name: "FK__StudentScore__SubjectScore",
                table: "StudentScores",
                column: "SubjectScoreID",
                principalTable: "SubjectScores",
                principalColumn: "SubjectScoreID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK__StudentScore__SubjectScore",
                table: "StudentScores");

            migrationBuilder.RenameColumn(
                name: "SubjectScoreID",
                table: "StudentScores",
                newName: "SubjectID");

            migrationBuilder.RenameIndex(
                name: "IX_StudentScores_SubjectScoreID",
                table: "StudentScores",
                newName: "IX_StudentScores_SubjectID");

            migrationBuilder.AddForeignKey(
                name: "FK__StudentScore__Subject",
                table: "StudentScores",
                column: "SubjectID",
                principalTable: "Subjects",
                principalColumn: "SubjectID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
