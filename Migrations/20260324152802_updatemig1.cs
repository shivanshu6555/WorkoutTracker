using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WorkoutTracker.Migrations
{
    /// <inheritdoc />
    public partial class updatemig1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Name", "WeightIncrement" },
                values: new object[] { "Pull Up", 0m });

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Name", "WeightIncrement" },
                values: new object[] { "Lat Pulldown", 5m });

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "MuscleGroup", "Name" },
                values: new object[] { "Back", "Seated Cable Row" });

            migrationBuilder.InsertData(
                table: "Exercises",
                columns: new[] { "Id", "MuscleGroup", "Name", "Type", "WeightIncrement" },
                values: new object[,]
                {
                    { 4, "Back", "Barbell Bent-Over Row", 1, 5m },
                    { 5, "Back", "T-Bar Row", 1, 5m },
                    { 6, "Back", "Dumbbell Row", 1, 5m },
                    { 7, "Biceps", "Barbell Bicep Curl", 1, 5m },
                    { 8, "Biceps", "Preacher Curl", 1, 2.5m },
                    { 9, "Biceps", "Cable Reverse Curl", 1, 2.5m },
                    { 10, "Biceps", "Dumbbell Hammer Curl", 1, 5m },
                    { 11, "Biceps", "Incline Dumbbell Curl", 1, 5m },
                    { 12, "Legs", "Smith Machine Squat", 1, 5m },
                    { 13, "Legs", "Seated Leg Curl", 1, 5m },
                    { 14, "Legs", "Leg Extension", 1, 5m },
                    { 15, "Legs", "Standing Calf Raise", 1, 5m },
                    { 16, "Legs", "Leg Press", 1, 10m },
                    { 17, "Shoulders", "Dumbbell Shoulder Press", 1, 5m },
                    { 18, "Shoulders", "Rear Delt Fly", 1, 2.5m },
                    { 19, "Shoulders", "Cable Face Pull", 1, 2.5m },
                    { 20, "Shoulders", "Dumbbell Shrug", 1, 5m },
                    { 21, "Chest", "Barbell Bench Press", 1, 5m },
                    { 22, "Chest", "Smith Machine Incline Press", 1, 5m },
                    { 23, "Chest", "Decline Chest Press Machine", 1, 5m },
                    { 24, "Chest", "Pec Deck Fly", 1, 5m },
                    { 25, "Chest", "Dumbbell Bench Press", 1, 5m },
                    { 26, "Triceps", "Cable Overhead Tricep Extension", 1, 2.5m },
                    { 27, "Triceps", "Cable Tricep Pushdown", 1, 2.5m },
                    { 28, "Triceps", "Dumbbell Tricep Kickback", 1, 2.5m },
                    { 29, "Core", "Machine Crunch", 1, 5m },
                    { 30, "Core", "Hanging Leg Raise", 2, 0m }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 30);

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Name", "WeightIncrement" },
                values: new object[] { "Pull up", 5.0m });

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Name", "WeightIncrement" },
                values: new object[] { "Machine Pulldown", 10.0m });

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "MuscleGroup", "Name" },
                values: new object[] { "Biceps", "Hammer Curls" });
        }
    }
}
