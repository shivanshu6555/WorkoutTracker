using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders; // Add this using directive

namespace WorkoutTracker.Models
{
    public class WorkoutDBContext : DbContext
    {
        public WorkoutDBContext(DbContextOptions<WorkoutDBContext> options)
             : base(options) { }

        public DbSet<Exercise> Exercises { get; set; }
        public DbSet<WorkoutSession> Sessions { get; set; }
        public DbSet<WorkoutSet> Sets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<WorkoutSet>()
                .Property(s => s.Weight)
                .HasPrecision(5, 2);

            modelBuilder.Entity<WorkoutSet>()
                .HasOne(s => s.Session)
                .WithMany(ws => ws.Sets)
                .HasForeignKey(s => s.WorkoutSessionId)
                .OnDelete(DeleteBehavior.Cascade);

            // UPDATED SEED DATA: Added MuscleGroup and WeightIncrement
            modelBuilder.Entity<Exercise>().HasData(
                // BACK
                new Exercise { Id = 1, Name = "Pull Up", Type = ExerciseType.Bodyweight, MuscleGroup = "Back", WeightIncrement = 0m },
                new Exercise { Id = 2, Name = "Lat Pulldown", Type = ExerciseType.Weighted, MuscleGroup = "Back", WeightIncrement = 5m },
                new Exercise { Id = 3, Name = "Seated Cable Row", Type = ExerciseType.Weighted, MuscleGroup = "Back", WeightIncrement = 5m },
                new Exercise { Id = 4, Name = "Barbell Bent-Over Row", Type = ExerciseType.Weighted, MuscleGroup = "Back", WeightIncrement = 5m },
                new Exercise { Id = 5, Name = "T-Bar Row", Type = ExerciseType.Weighted, MuscleGroup = "Back", WeightIncrement = 5m },
                new Exercise { Id = 6, Name = "Dumbbell Row", Type = ExerciseType.Weighted, MuscleGroup = "Back", WeightIncrement = 5m },

                // BICEPS
                new Exercise { Id = 7, Name = "Barbell Bicep Curl", Type = ExerciseType.Weighted, MuscleGroup = "Biceps", WeightIncrement = 5m },
                new Exercise { Id = 8, Name = "Preacher Curl", Type = ExerciseType.Weighted, MuscleGroup = "Biceps", WeightIncrement = 2.5m },
                new Exercise { Id = 9, Name = "Cable Reverse Curl", Type = ExerciseType.Weighted, MuscleGroup = "Biceps", WeightIncrement = 2.5m },
                new Exercise { Id = 10, Name = "Dumbbell Hammer Curl", Type = ExerciseType.Weighted, MuscleGroup = "Biceps", WeightIncrement = 5m },
                new Exercise { Id = 11, Name = "Incline Dumbbell Curl", Type = ExerciseType.Weighted, MuscleGroup = "Biceps", WeightIncrement = 5m },

                // LEGS
                new Exercise { Id = 12, Name = "Smith Machine Squat", Type = ExerciseType.Weighted, MuscleGroup = "Legs", WeightIncrement = 5m },
                new Exercise { Id = 13, Name = "Seated Leg Curl", Type = ExerciseType.Weighted, MuscleGroup = "Legs", WeightIncrement = 5m },
                new Exercise { Id = 14, Name = "Leg Extension", Type = ExerciseType.Weighted, MuscleGroup = "Legs", WeightIncrement = 5m },
                new Exercise { Id = 15, Name = "Standing Calf Raise", Type = ExerciseType.Weighted, MuscleGroup = "Legs", WeightIncrement = 5m },
                new Exercise { Id = 16, Name = "Leg Press", Type = ExerciseType.Weighted, MuscleGroup = "Legs", WeightIncrement = 10m },

                // SHOULDERS
                new Exercise { Id = 17, Name = "Dumbbell Shoulder Press", Type = ExerciseType.Weighted, MuscleGroup = "Shoulders", WeightIncrement = 5m },
                new Exercise { Id = 18, Name = "Rear Delt Fly", Type = ExerciseType.Weighted, MuscleGroup = "Shoulders", WeightIncrement = 2.5m },
                new Exercise { Id = 19, Name = "Cable Face Pull", Type = ExerciseType.Weighted, MuscleGroup = "Shoulders", WeightIncrement = 2.5m },
                new Exercise { Id = 20, Name = "Dumbbell Shrug", Type = ExerciseType.Weighted, MuscleGroup = "Shoulders", WeightIncrement = 5m },
                new Exercise { Id = 32, Name = "Shoulder Press Machine", Type = ExerciseType.Weighted, MuscleGroup = "Shoulders", WeightIncrement = 5m },

                // CHEST
                new Exercise { Id = 21, Name = "Barbell Bench Press", Type = ExerciseType.Weighted, MuscleGroup = "Chest", WeightIncrement = 5m },
                new Exercise { Id = 22, Name = "Smith Machine Incline Press", Type = ExerciseType.Weighted, MuscleGroup = "Chest", WeightIncrement = 5m },
                new Exercise { Id = 23, Name = "Decline Chest Press Machine", Type = ExerciseType.Weighted, MuscleGroup = "Chest", WeightIncrement = 5m },
                new Exercise { Id = 24, Name = "Pec Deck Fly", Type = ExerciseType.Weighted, MuscleGroup = "Chest", WeightIncrement = 5m },
                new Exercise { Id = 25, Name = "Dumbbell Bench Press", Type = ExerciseType.Weighted, MuscleGroup = "Chest", WeightIncrement = 5m },
                new Exercise { Id = 31, Name = "Incline Dumbbell Press", Type = ExerciseType.Weighted, MuscleGroup = "Chest", WeightIncrement = 5m },

                // TRICEPS
                new Exercise { Id = 26, Name = "Cable Overhead Tricep Extension", Type = ExerciseType.Weighted, MuscleGroup = "Triceps", WeightIncrement = 2.5m },
                new Exercise { Id = 27, Name = "Cable Tricep Pushdown", Type = ExerciseType.Weighted, MuscleGroup = "Triceps", WeightIncrement = 2.5m },
                new Exercise { Id = 28, Name = "Dumbbell Tricep Kickback", Type = ExerciseType.Weighted, MuscleGroup = "Triceps", WeightIncrement = 2.5m },

                // CORE
                new Exercise { Id = 29, Name = "Machine Crunch", Type = ExerciseType.Weighted, MuscleGroup = "Core", WeightIncrement = 5m },
                new Exercise { Id = 30, Name = "Hanging Leg Raise", Type = ExerciseType.Bodyweight, MuscleGroup = "Core", WeightIncrement = 0m }
            );
        }
    }
}
