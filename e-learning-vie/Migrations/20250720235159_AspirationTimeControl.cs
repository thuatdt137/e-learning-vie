using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace e_learning_vie.Migrations
{
    /// <inheritdoc />
    public partial class AspirationTimeControl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "SchoolType",
                table: "Schools",
                type: "int",
                maxLength: 20,
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Aspirations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "AspirationEditDeadline",
                table: "AcademicYears",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "AspirationRegistrationEndDate",
                table: "AcademicYears",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "AspirationRegistrationStartDate",
                table: "AcademicYears",
                type: "date",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Aspirations");

            migrationBuilder.DropColumn(
                name: "AspirationEditDeadline",
                table: "AcademicYears");

            migrationBuilder.DropColumn(
                name: "AspirationRegistrationEndDate",
                table: "AcademicYears");

            migrationBuilder.DropColumn(
                name: "AspirationRegistrationStartDate",
                table: "AcademicYears");

            migrationBuilder.AlterColumn<string>(
                name: "SchoolType",
                table: "Schools",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldMaxLength: 20);
        }
    }
}
