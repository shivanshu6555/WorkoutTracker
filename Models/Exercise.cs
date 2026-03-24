namespace WorkoutTracker.Models
{
    public class Exercise
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public ExerciseType Type { get; set; }

        // NEW PROPERTIES
        public string MuscleGroup { get; set; } = string.Empty;
        public decimal WeightIncrement { get; set; } = 5.0m; // Default to 5

        // Navigation Property
        public ICollection<WorkoutSet> Sets { get; set; } = new List<WorkoutSet>();
    }
}