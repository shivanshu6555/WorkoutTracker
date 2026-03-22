using Microsoft.EntityFrameworkCore;
using WorkoutTracker.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddDbContext<WorkoutDBContext>(options => options.UseAzureSql(builder.Configuration.GetConnectionString("DevConnection")));
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();


var api = app.MapGroup("/api");

// 1. Get all exercises (for your dropdown menu)
api.MapGet("/exercises", async (WorkoutDBContext db) =>
{
    return await db.Exercises.AsNoTracking().ToListAsync();
});

// 2. Start a new workout session
api.MapPost("/sessions", async (StartSessionRequest req, WorkoutDBContext db) =>
{
    var session = new WorkoutSession
    {
        Date = DateTime.UtcNow,
        OverallNotes = req.OverallNotes
    };

    db.Sessions.Add(session);
    await db.SaveChangesAsync();

    return Results.Created($"/api/sessions/{session.Id}", session);
});

// 3. Log a set (The "Quick Entry" endpoint)
api.MapPost("/sessions/{sessionId:int}/sets", async (int sessionId, LogSetRequest req, WorkoutDBContext db) =>
{
    // Figure out the set order (e.g., is this set 1, 2, or 3 for this exercise today?)
    var currentSetCount = await db.Sets
        .Where(s => s.WorkoutSessionId == sessionId && s.ExerciseId == req.ExerciseId)
        .CountAsync();

    var newSet = new WorkoutSet
    {
        WorkoutSessionId = sessionId,
        ExerciseId = req.ExerciseId,
        SetOrder = currentSetCount + 1,
        Weight = req.Weight,
        Reps = req.Reps,
        TargetReps = req.TargetReps,
        IsDropSet = req.IsDropSet,
        Notes = req.Notes
    };

    db.Sets.Add(newSet);
    await db.SaveChangesAsync();

    return Results.Ok(newSet);
});

// 4. Get last session's sets for an exercise (Powers the "Ghost Text" / "Last Time" feature)
api.MapGet("/exercises/{exerciseId:int}/history", async (int exerciseId, WorkoutDBContext db) =>
{
    // Find the most recent session where you performed this exercise
    var lastSessionWithExercise = await db.Sets
        .Where(s => s.ExerciseId == exerciseId)
        .OrderByDescending(s => s.Session.Date)
        .Select(s => s.WorkoutSessionId)
        .FirstOrDefaultAsync();

    if (lastSessionWithExercise == 0)
        return Results.NotFound("No history found for this exercise.");

    // Get all sets from that specific session
    var previousSets = await db.Sets
        .Where(s => s.WorkoutSessionId == lastSessionWithExercise && s.ExerciseId == exerciseId)
        .OrderBy(s => s.SetOrder)
        .Select(s => new { s.Weight, s.Reps, s.TargetReps, s.IsDropSet })
        .ToListAsync();

    return Results.Ok(previousSets);
});

//app.MapGet("/api/test-db", async (WorkoutDBContext db) =>
//{
//    try
//    {
//        // 1. Test Write: Add a dummy exercise
//        var testExercise = new Exercise
//        {
//            Name = "Test Bench Press " + Guid.NewGuid().ToString().Substring(0, 4),
//            Type = ExerciseType.Weighted
//        };

//        db.Exercises.Add(testExercise);
//        await db.SaveChangesAsync();

//        // 2. Test Read: Fetch the exercise we just added
//        var savedExercise = await db.Exercises
//            .OrderByDescending(e => e.Id)
//            .FirstOrDefaultAsync();

//        // 3. Test Delete: Clean up our mess
//        if (savedExercise != null)
//        {
//            db.Exercises.Remove(savedExercise);
//            await db.SaveChangesAsync();
//        }

//        return Results.Ok(new
//        {
//            Message = "Database connection successful! Classes map correctly.",
//            CreatedAndReadExercise = savedExercise?.Name
//        });
//    }
//    catch (Exception ex)
//    {
//        return Results.Problem($"Database test failed: {ex.Message}");
//    }
//});

app.Run();

public record StartSessionRequest(string? OverallNotes);

public record LogSetRequest(
    int ExerciseId,
    decimal? Weight,
    int Reps,
    int? TargetReps,
    bool IsDropSet,
    string? Notes
);