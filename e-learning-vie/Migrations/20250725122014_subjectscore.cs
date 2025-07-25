using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace e_learning_vie.Migrations
{
    /// <inheritdoc />
    public partial class subjectscore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK__StudentScore__ScoreType",
                table: "StudentScores");

            migrationBuilder.DropIndex(
                name: "IX_StudentScores_ScoreTypeID",
                table: "StudentScores");

            migrationBuilder.DropColumn(
                name: "ScoreTypeID",
                table: "StudentScores");

            migrationBuilder.CreateTable(
                name: "SubjectScores",
                columns: table => new
                {
                    SubjectScoreID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubjectID = table.Column<int>(type: "int", nullable: false),
                    ScoreTypeID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__SubjectScore", x => x.SubjectScoreID);
                    table.ForeignKey(
                        name: "FK__SubjectScore__ScoreType",
                        column: x => x.ScoreTypeID,
                        principalTable: "ScoreTypes",
                        principalColumn: "ScoreID");
                    table.ForeignKey(
                        name: "FK__SubjectScore__Subject",
                        column: x => x.SubjectID,
                        principalTable: "Subjects",
                        principalColumn: "SubjectID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_SubjectScores_ScoreTypeID",
                table: "SubjectScores",
                column: "ScoreTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_SubjectScores_SubjectID",
                table: "SubjectScores",
                column: "SubjectID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SubjectScores");

            migrationBuilder.AddColumn<int>(
                name: "ScoreTypeID",
                table: "StudentScores",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_StudentScores_ScoreTypeID",
                table: "StudentScores",
                column: "ScoreTypeID");

            migrationBuilder.AddForeignKey(
                name: "FK__StudentScore__ScoreType",
                table: "StudentScores",
                column: "ScoreTypeID",
                principalTable: "ScoreTypes",
                principalColumn: "ScoreID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
