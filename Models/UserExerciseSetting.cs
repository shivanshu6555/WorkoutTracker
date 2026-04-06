namespace WorkoutTracker.Models
{
    public class UserExerciseSetting
    {
        public int Id { get; set; }

        // Who is making the change?
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        // Which exercise are they overriding?
        public int ExerciseId { get; set; }
        public Exercise Exercise { get; set; } = null!;

        // What is their custom increment?
        public decimal WeightIncrement { get; set; }
    }
}
