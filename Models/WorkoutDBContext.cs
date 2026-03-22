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

            // Configure decimal precision for Weight (e.g., 12.5 lbs)
            modelBuilder.Entity<WorkoutSet>()
                .Property(s => s.Weight)
                .HasPrecision(5, 2); // Replace HasColumnType with HasPrecision

            // Ensure if a WorkoutSession is deleted, its Sets are deleted too
            modelBuilder.Entity<WorkoutSet>()
                .HasOne(s => s.Session)
                .WithMany(ws => ws.Sets)
                .HasForeignKey(s => s.WorkoutSessionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed some initial data so you can test right away
            modelBuilder.Entity<Exercise>().HasData(
                new Exercise { Id = 1, Name = "Pull up", Type = ExerciseType.Bodyweight },
                new Exercise { Id = 2, Name = "Machine Pulldown", Type = ExerciseType.Weighted },
                new Exercise { Id = 3, Name = "Hammer Curls", Type = ExerciseType.Weighted }
            );
        }
    }
}
