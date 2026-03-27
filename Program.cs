using Microsoft.EntityFrameworkCore;
using WorkoutTracker.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});
builder.Services.AddOpenApi();
builder.Services.AddDbContext<WorkoutDBContext>(options => options.UseAzureSql(builder.Configuration.GetConnectionString("DevConnection")));
// --- JWT AUTHENTICATION SETUP ---
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!))
        };
    });
builder.Services.AddAuthorization();
// --------------------------------
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.UseAuthentication(); // NEW
app.UseAuthorization();  // NEW
app.UseCors();

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
}).RequireAuthorization();

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
}).RequireAuthorization();

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

// Define the shape of the login request

// 1. REGISTER ENDPOINT
app.MapPost("/api/register", async (AuthRequest req, WorkoutDBContext db) =>
{
    // Normalize phone number by removing spaces
    var phone = req.PhoneNumber.Replace(" ", "");

    if (await db.Users.AnyAsync(u => u.PhoneNumber == phone))
        return Results.BadRequest("Phone number already registered.");

    var user = new User
    {
        PhoneNumber = phone,
        // BCrypt magically handles the salt and hashing!
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password)
    };

    db.Users.Add(user);
    await db.SaveChangesAsync();

    return Results.Ok(new { message = "Registration successful!" });
});


// 2. LOGIN ENDPOINT
app.MapPost("/api/login", async (AuthRequest req, IConfiguration config, WorkoutDBContext db) =>
{
    var phone = req.PhoneNumber.Replace(" ", "");
    var user = await db.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phone);

    // Verify the user exists and the password matches the hash
    if (user == null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
        return Results.Unauthorized();

    // Generate the 30-Day JWT "Wristband"
    var tokenHandler = new JwtSecurityTokenHandler();
    // Notice we use 'config' here instead of 'builder.Configuration'
    var key = Encoding.UTF8.GetBytes(config["JwtSettings:SecretKey"]!);
    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.MobilePhone, user.PhoneNumber)
        }),
        Expires = DateTime.UtcNow.AddDays(30), // Keeps them logged in for a month
        Issuer = config["JwtSettings:Issuer"],
        Audience = config["JwtSettings:Audience"],
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
    };

    var token = tokenHandler.CreateToken(tokenDescriptor);
    var jwtString = tokenHandler.WriteToken(token);

    // Send the token back to React
    return Results.Ok(new { token = jwtString });
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
public record AuthRequest(string PhoneNumber, string Password);
