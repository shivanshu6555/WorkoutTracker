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

        // NEW: Foreign Key linking to the User
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        // Navigation Property
        public ICollection<WorkoutSet> Sets { get; set; } = new List<WorkoutSet>();
    }
}
