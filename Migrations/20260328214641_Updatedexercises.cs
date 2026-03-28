using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WorkoutTracker.Migrations
{
    /// <inheritdoc />
    public partial class Updatedexercises : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Exercises",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    MuscleGroup = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WeightIncrement = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exercises", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Exercises_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Sessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DurationInMinutes = table.Column<int>(type: "int", nullable: true),
                    OverallNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sessions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Sets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkoutSessionId = table.Column<int>(type: "int", nullable: false),
                    ExerciseId = table.Column<int>(type: "int", nullable: false),
                    SetOrder = table.Column<int>(type: "int", nullable: false),
                    Weight = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    Reps = table.Column<int>(type: "int", nullable: false),
                    TargetReps = table.Column<int>(type: "int", nullable: true),
                    IsDropSet = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sets_Exercises_ExerciseId",
                        column: x => x.ExerciseId,
                        principalTable: "Exercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Sets_Sessions_WorkoutSessionId",
                        column: x => x.WorkoutSessionId,
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Exercises",
                columns: new[] { "Id", "MuscleGroup", "Name", "Type", "UserId", "WeightIncrement" },
                values: new object[,]
                {
                    { 1, "Back", "Pull Up", 2, null, 0m },
                    { 2, "Back", "Lat Pulldown", 1, null, 5m },
                    { 3, "Back", "Seated Cable Row", 1, null, 5m },
                    { 4, "Back", "Barbell Bent-Over Row", 1, null, 5m },
                    { 5, "Back", "T-Bar Row", 1, null, 5m },
                    { 6, "Back", "Dumbbell Row", 1, null, 5m },
                    { 7, "Biceps", "Barbell Bicep Curl", 1, null, 5m },
                    { 8, "Biceps", "Preacher Curl", 1, null, 2.5m },
                    { 9, "Biceps", "Cable Reverse Curl", 1, null, 2.5m },
                    { 10, "Biceps", "Dumbbell Hammer Curl", 1, null, 5m },
                    { 11, "Biceps", "Incline Dumbbell Curl", 1, null, 5m },
                    { 12, "Legs", "Smith Machine Squat", 1, null, 5m },
                    { 13, "Legs", "Seated Leg Curl", 1, null, 5m },
                    { 14, "Legs", "Leg Extension", 1, null, 5m },
                    { 15, "Legs", "Standing Calf Raise", 1, null, 5m },
                    { 16, "Legs", "Leg Press", 1, null, 10m },
                    { 17, "Shoulders", "Dumbbell Shoulder Press", 1, null, 5m },
                    { 18, "Shoulders", "Rear Delt Fly", 1, null, 2.5m },
                    { 19, "Shoulders", "Cable Face Pull", 1, null, 2.5m },
                    { 20, "Shoulders", "Dumbbell Shrug", 1, null, 5m },
                    { 21, "Chest", "Barbell Bench Press", 1, null, 5m },
                    { 22, "Chest", "Smith Machine Incline Press", 1, null, 5m },
                    { 23, "Chest", "Decline Chest Press Machine", 1, null, 5m },
                    { 24, "Chest", "Pec Deck Fly", 1, null, 5m },
                    { 25, "Chest", "Dumbbell Bench Press", 1, null, 5m },
                    { 26, "Triceps", "Cable Overhead Tricep Extension", 1, null, 2.5m },
                    { 27, "Triceps", "Cable Tricep Pushdown", 1, null, 2.5m },
                    { 28, "Triceps", "Dumbbell Tricep Kickback", 1, null, 2.5m },
                    { 29, "Core", "Machine Crunch", 1, null, 5m },
                    { 30, "Core", "Hanging Leg Raise", 2, null, 0m },
                    { 31, "Chest", "Incline Dumbbell Press", 1, null, 5m },
                    { 32, "Shoulders", "Shoulder Press Machine", 1, null, 5m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_UserId",
                table: "Exercises",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_UserId",
                table: "Sessions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Sets_ExerciseId",
                table: "Sets",
                column: "ExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_Sets_WorkoutSessionId",
                table: "Sets",
                column: "WorkoutSessionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Sets");

            migrationBuilder.DropTable(
                name: "Exercises");

            migrationBuilder.DropTable(
                name: "Sessions");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
