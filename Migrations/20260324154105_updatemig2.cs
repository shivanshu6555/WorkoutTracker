using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WorkoutTracker.Migrations
{
    /// <inheritdoc />
    public partial class updatemig2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Exercises",
                columns: new[] { "Id", "MuscleGroup", "Name", "Type", "WeightIncrement" },
                values: new object[,]
                {
                    { 31, "Chest", "Incline Dumbbell Press", 1, 5m },
                    { 32, "Shoulders", "Shoulder Press Machine", 1, 5m }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 32);
        }
    }
}
