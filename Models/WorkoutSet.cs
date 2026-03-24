namespace WorkoutTracker.Models
{
    public class WorkoutSet
    {
        public int Id { get; set; }

        // Foreign Keys
        public int WorkoutSessionId { get; set; }
        public WorkoutSession Session { get; set; } = null!;

        public int ExerciseId { get; set; }
        public Exercise Exercise { get; set; } = null!;

        public int SetOrder { get; set; }
        public decimal? Weight { get; set; }
        public int Reps { get; set; }
        public int? TargetReps { get; set; }
        public bool IsDropSet { get; set; }
        public string? Notes { get; set; }

        // NEW PROPERTY
        public string Unit { get; set; } = "lbs";
    }
}