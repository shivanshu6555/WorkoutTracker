namespace WorkoutTracker.Models
{
    public class WorkoutSession
    {
        public int Id { get; set; }
        public DateTime Date { get; set; } = DateTime.UtcNow;

        // Optional: Track how long the workout took
        public int? DurationInMinutes { get; set; }

        // For overall session notes like "Felt tired today"
        public string? OverallNotes { get; set; }

        // Navigation Property
        public ICollection<WorkoutSet> Sets { get; set; } = new List<WorkoutSet>();
    }
}
