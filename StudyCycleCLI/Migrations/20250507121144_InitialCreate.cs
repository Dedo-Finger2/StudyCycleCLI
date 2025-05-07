using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudyCycleCLI.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StudyCycles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    DailyStudyHours = table.Column<double>(type: "REAL", nullable: false),
                    WeeklyStudyHours = table.Column<double>(type: "REAL", nullable: false),
                    FactorOne = table.Column<int>(type: "INTEGER", nullable: false),
                    FactorTwo = table.Column<int>(type: "INTEGER", nullable: false),
                    FactorThree = table.Column<int>(type: "INTEGER", nullable: false),
                    CompletedTimes = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastStudiedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudyCycles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StudyCycleSubjects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StudyCycleId = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Difficulty = table.Column<int>(type: "INTEGER", nullable: false),
                    AmountOfContent = table.Column<int>(type: "INTEGER", nullable: false),
                    Weight = table.Column<double>(type: "REAL", nullable: false),
                    StudiedHours = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxStudyHours = table.Column<int>(type: "INTEGER", nullable: false),
                    CompletedTimes = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastStudiedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudyCycleSubjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudyCycleSubjects_StudyCycles_StudyCycleId",
                        column: x => x.StudyCycleId,
                        principalTable: "StudyCycles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StudyCycleSubjects_StudyCycleId",
                table: "StudyCycleSubjects",
                column: "StudyCycleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StudyCycleSubjects");

            migrationBuilder.DropTable(
                name: "StudyCycles");
        }
    }
}
