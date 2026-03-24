using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkoutTracker.Migrations
{
    /// <inheritdoc />
    public partial class updatemig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Unit",
                table: "Sets",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MuscleGroup",
                table: "Exercises",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "WeightIncrement",
                table: "Exercises",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "MuscleGroup", "WeightIncrement" },
                values: new object[] { "Back", 5.0m });

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "MuscleGroup", "WeightIncrement" },
                values: new object[] { "Back", 10.0m });

            migrationBuilder.UpdateData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "MuscleGroup", "WeightIncrement" },
                values: new object[] { "Biceps", 5.0m });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Unit",
                table: "Sets");

            migrationBuilder.DropColumn(
                name: "MuscleGroup",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "WeightIncrement",
                table: "Exercises");
        }
    }
}
