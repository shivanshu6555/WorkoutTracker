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
app.UseCors();           // 1. Let the browser in
app.UseAuthentication(); // 2. Check their JWT wristband
app.UseAuthorization();  // 3. Verify their permissions
app.MapControllers();    // 4. Send them to the endpoint


var api = app.MapGroup("/api");

// 1. Get all exercises (With User Overrides applied!)
api.MapGet("/exercises", async (ClaimsPrincipal user, WorkoutDBContext db) =>
{
    var userIdString = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (userIdString == null) return Results.Unauthorized();
    int userId = int.Parse(userIdString);

    // Grab the exercises (Global + Custom)
    var exercises = await db.Exercises.AsNoTracking()
        .Where(e => e.UserId == null || e.UserId == userId)
        .ToListAsync();

    // Grab any specific machine overrides this user has set
    var overrides = await db.UserExerciseSettings.AsNoTracking()
        .Where(s => s.UserId == userId)
        .ToDictionaryAsync(s => s.ExerciseId, s => s.WeightIncrement);

    // Merge them together seamlessly
    var customizedExercises = exercises.Select(e => new {
        e.Id,
        e.Name,
        e.MuscleGroup,
        e.Type,
        e.UserId,
        // If they have an override, use it! Otherwise, stick to the default.
        WeightIncrement = overrides.ContainsKey(e.Id) ? overrides[e.Id] : e.WeightIncrement
    });

    return Results.Ok(customizedExercises);
}).RequireAuthorization();


// 2. Add a custom exercise
api.MapPost("/exercises", async (CreateExerciseRequest req, ClaimsPrincipal user, WorkoutDBContext db) =>
{
    var userIdString = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (userIdString == null) return Results.Unauthorized();

    var exercise = new Exercise
    {
        Name = req.Name,
        MuscleGroup = req.MuscleGroup,
        WeightIncrement = req.WeightIncrement,
        Type = ExerciseType.Weighted,
        UserId = int.Parse(userIdString) // <--- Lock this exercise to this exact user
    };

    db.Exercises.Add(exercise);
    await db.SaveChangesAsync();

    return Results.Ok(exercise);
}).RequireAuthorization();

// 2. Start a new workout session
api.MapPost("/sessions", async (StartSessionRequest req, ClaimsPrincipal user, WorkoutDBContext db) =>
{
    // 1. Inspect the wristband and grab the User ID
    var userIdString = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    // If they don't have a valid wristband, kick them out
    if (userIdString == null) return Results.Unauthorized();

    // 2. Create the session tied directly to this specific user
    var newSession = new WorkoutSession
    {
        UserId = int.Parse(userIdString),
        OverallNotes = req.OverallNotes, // Use the safe record here!
        Date = DateTime.UtcNow
    };

    db.Sessions.Add(newSession);
    await db.SaveChangesAsync();

    return Results.Ok(newSession);
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

// 4. Get last session's sets for an exercise (SECURED)
api.MapGet("/exercises/{exerciseId:int}/history", async (int exerciseId, ClaimsPrincipal user, WorkoutDBContext db) =>
{
    var userIdString = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (userIdString == null) return Results.Unauthorized();
    int userId = int.Parse(userIdString);

    var lastSessionWithExercise = await db.Sets
        // FIX: Ensure the set belongs to a session owned by THIS user!
        .Where(s => s.ExerciseId == exerciseId && s.Session.UserId == userId)
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
            Date = s.Session.Date
        })
        .ToListAsync();

    return Results.Ok(previousSets);
}).RequireAuthorization(); // <--- Secured!

// 5. Get Progression Analytics (SECURED)
api.MapGet("/exercises/{exerciseId:int}/progression", async (int exerciseId, ClaimsPrincipal user, WorkoutDBContext db) =>
{
    var userIdString = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (userIdString == null) return Results.Unauthorized();
    int userId = int.Parse(userIdString);

    var rawStats = await db.Sets
        // FIX: Ensure we only calculate volume for THIS user!
        .Where(s => s.ExerciseId == exerciseId && s.Session.UserId == userId && s.Weight > 0)
        .GroupBy(s => s.Session.Date.Date)
        .Select(g => new {
            RawDate = g.Key,
            TotalVolume = g.Sum(s => s.Weight * s.Reps)
        })
        .OrderBy(s => s.RawDate)
        .Take(10)
        .ToListAsync();

    var formattedStats = rawStats.Select(s => new {
        Date = s.RawDate.ToString("MMM dd"),
        TotalVolume = s.TotalVolume
    }).ToList();

    return Results.Ok(formattedStats);
}).RequireAuthorization(); // <--- Secured!

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

// NEW: Update Weight Increment
api.MapPut("/exercises/{id:int}/increment", async (int id, UpdateIncrementRequest req, ClaimsPrincipal user, WorkoutDBContext db) =>
{
    var userIdString = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (userIdString == null) return Results.Unauthorized();
    int userId = int.Parse(userIdString);

    var exercise = await db.Exercises.FindAsync(id);
    if (exercise == null) return Results.NotFound();

    // If it's their own custom exercise, just update the main table directly
    if (exercise.UserId == userId)
    {
        exercise.WeightIncrement = req.NewIncrement;
    }
    else
    {
        // If it's a global exercise, save an override for this user only
        var setting = await db.UserExerciseSettings
            .FirstOrDefaultAsync(s => s.UserId == userId && s.ExerciseId == id);

        if (setting != null)
        {
            setting.WeightIncrement = req.NewIncrement; // Update existing override
        }
        else
        {
            // Create a brand new override
            db.UserExerciseSettings.Add(new UserExerciseSetting
            {
                UserId = userId,
                ExerciseId = id,
                WeightIncrement = req.NewIncrement
            });
        }
    }

    await db.SaveChangesAsync();
    return Results.Ok(new { message = "Increment updated successfully!" });
}).RequireAuthorization();

// 6. Get Full Master History (Grouped by Session/Date)
api.MapGet("/history", async (ClaimsPrincipal user, WorkoutDBContext db) =>
{
    var userIdString = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (userIdString == null) return Results.Unauthorized();
    int userId = int.Parse(userIdString);

    var history = await db.Sessions
        .AsNoTracking()
        .Where(s => s.UserId == userId)
        // Bring the newest workouts to the top
        .OrderByDescending(s => s.Date)
        .Select(s => new {
            s.Id,
            Date = s.Date,
            s.OverallNotes,
            // Grab every set in this session and get the actual Exercise Name
            Sets = s.Sets.OrderBy(set => set.SetOrder).Select(set => new {
                ExerciseName = set.Exercise.Name,
                MuscleGroup = set.Exercise.MuscleGroup,
                set.Weight,
                set.Reps,
                set.IsDropSet,
                set.Unit
            })
        })
        .ToListAsync();

    return Results.Ok(history);
}).RequireAuthorization();
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
public record UpdateIncrementRequest(decimal NewIncrement);
