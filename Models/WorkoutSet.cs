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

        // The order of the set within the specific exercise for that session
        public int SetOrder { get; set; }

        // Nullable because bodyweight exercises (like your pull-ups) won't have a weight
        public decimal? Weight { get; set; }

        // The reps you actually completed
        public int Reps { get; set; }

        // Captures your "14(12😑)" scenario. Target = 14, Reps = 12.
        public int? TargetReps { get; set; }

        // Flags the set as a drop set (like your hammer curls)
        public bool IsDropSet { get; set; }

        // Optional: Notes for a specific set (e.g., "Left shoulder felt weird")
        public string? Notes { get; set; }
    }
}
