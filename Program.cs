using Microsoft.EntityFrameworkCore;
using WorkoutTracker.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddDbContext<WorkoutDBContext>(options => options.UseAzureSql(builder.Configuration.GetConnectionString("DevConnection")));
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

var api = app.MapGroup("/api");

// 1. Get all exercises
api.MapGet("/exercises", async (WorkoutDBContext db) =>
{
    return await db.Exercises.AsNoTracking().ToListAsync();
});

// NEW: Add a custom exercise
api.MapPost("/exercises", async (CreateExerciseRequest req, WorkoutDBContext db) =>
{
    var exercise = new Exercise
    {
        Name = req.Name,
        MuscleGroup = req.MuscleGroup,
        WeightIncrement = req.WeightIncrement,
        Type = ExerciseType.Weighted // Defaulting custom exercises to Weighted
    };

    db.Exercises.Add(exercise);
    await db.SaveChangesAsync();

    return Results.Ok(exercise);
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

// 3. Log a set
api.MapPost("/sessions/{sessionId:int}/sets", async (int sessionId, LogSetRequest req, WorkoutDBContext db) =>
{
    var currentSetCount = await db.Sets
        .Where(s => s.WorkoutSessionId == sessionId && s.ExerciseId == req.ExerciseId)
        .CountAsync();

    var newSet = new WorkoutSet
    {
        WorkoutSessionId = sessionId,
        ExerciseId = req.ExerciseId,
        SetOrder = currentSetCount + 1,
        Weight = req.Weight,           // Maps directly to your decimal?
        Reps = req.Reps,
        TargetReps = req.TargetReps,   // Maps directly to your int?
        IsDropSet = req.IsDropSet,
        Notes = req.Notes,
        Unit = req.Unit ?? "lbs"       // Save the unit from React
    };

    db.Sets.Add(newSet);
    await db.SaveChangesAsync();

    return Results.Ok(newSet);
});

// 4. Get last session's sets for an exercise
api.MapGet("/exercises/{exerciseId:int}/history", async (int exerciseId, WorkoutDBContext db) =>
{
    var lastSessionWithExercise = await db.Sets
        .Where(s => s.ExerciseId == exerciseId)
        .OrderByDescending(s => s.Session.Date)
        .Select(s => s.WorkoutSessionId)
        .FirstOrDefaultAsync();

    if (lastSessionWithExercise == 0)
        return Results.NotFound("No history found for this exercise.");

    var previousSets = await db.Sets
        .Where(s => s.WorkoutSessionId == lastSessionWithExercise && s.ExerciseId == exerciseId)
        .OrderBy(s => s.SetOrder)
        .Select(s => new {
            s.Weight,
            s.Reps,
            s.TargetReps,
            s.IsDropSet,
            Unit = s.Unit,
            Date = s.Session.Date // Magically pulls the date from the parent Session table!
        })
        .ToListAsync();

    return Results.Ok(previousSets);
});

// 5. Get Progression Analytics (Total Volume over time)
api.MapGet("/exercises/{exerciseId:int}/progression", async (int exerciseId, WorkoutDBContext db) =>
{
    // Step 1: Do the heavy lifting in SQL (Grouping, Summing, Sorting)
    var rawStats = await db.Sets
        .Where(s => s.ExerciseId == exerciseId && s.Weight > 0)
        .GroupBy(s => s.Session.Date.Date)
        .Select(g => new {
            RawDate = g.Key,
            TotalVolume = g.Sum(s => s.Weight * s.Reps)
        })
        .OrderBy(s => s.RawDate)
        .Take(10)
        .ToListAsync(); // <-- This executes the SQL query and brings data into C# memory

    // Step 2: Format the string in C# memory where .ToString() works perfectly
    var formattedStats = rawStats.Select(s => new {
        Date = s.RawDate.ToString("MMM dd"),
        TotalVolume = s.TotalVolume
    }).ToList();

    return Results.Ok(formattedStats);
});

// 6. Bulk Import Historical Data
api.MapPost("/import-history", async (List<BulkImportSession> history, WorkoutDBContext db) =>
{
    var allExercises = await db.Exercises.ToListAsync();
    int setsImported = 0;

    foreach (var sessionData in history)
    {
        // 1. Create the session
        var session = new WorkoutSession { Date = sessionData.Date, OverallNotes = sessionData.Notes };
        db.Sessions.Add(session);
        await db.SaveChangesAsync(); // Save immediately to generate the Session ID

        // 2. Add the sets
        int setOrder = 1;
        foreach (var setData in sessionData.Sets)
        {
            // Find the exercise ID by matching the string name
            var exercise = allExercises.FirstOrDefault(e => e.Name.ToLower() == setData.ExerciseName.ToLower());
            if (exercise == null) continue; // Skip if we mapped the name wrong

            var newSet = new WorkoutSet
            {
                WorkoutSessionId = session.Id,
                ExerciseId = exercise.Id,
                SetOrder = setOrder++,
                Weight = setData.Weight,
                Reps = setData.Reps,
                TargetReps = setData.TargetReps,
                IsDropSet = setData.IsDropSet,
                Unit = setData.Unit
            };
            db.Sets.Add(newSet);
            setsImported++;
        }
    }

    await db.SaveChangesAsync();
    return Results.Ok($"Successfully imported {history.Count} sessions and {setsImported} sets!");
});

app.Run();

// --- REQUEST RECORDS ---
public record StartSessionRequest(string? OverallNotes);

public record LogSetRequest(
    int ExerciseId,
    decimal? Weight,
    int Reps,
    int? TargetReps,
    bool IsDropSet,
    string? Notes,
    string? Unit
);

public record CreateExerciseRequest(
    string Name,
    string MuscleGroup,
    decimal WeightIncrement
);

// DTOs for Bulk Import
public record BulkImportSession(DateTime Date, string Notes, List<BulkImportSet> Sets);
public record BulkImportSet(string ExerciseName, decimal Weight, int Reps, int? TargetReps, bool IsDropSet, string Unit);