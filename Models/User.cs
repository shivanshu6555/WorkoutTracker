namespace WorkoutTracker.Models
{
    public class User
    {
        public int Id { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;

        // Navigation Property: One user has many workout sessions
        public ICollection<WorkoutSession> Sessions { get; set; } = new List<WorkoutSession>();
    }
}
