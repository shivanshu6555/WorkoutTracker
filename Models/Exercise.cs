namespace WorkoutTracker.Models
{
    public class Exercise
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public ExerciseType Type { get; set; }

        // Navigation Property
        public ICollection<WorkoutSet> Sets { get; set; } = new List<WorkoutSet>();
    }
}
