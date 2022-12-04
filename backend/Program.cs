using Crosswords.Db;
using Crosswords.Db.Models;
using Crosswords.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);
builder.Services
    .AddDbContextPool<CrosswordsContext>(options => options
        //.EnableThreadSafetyChecks(false)
        .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
        .UseNpgsql(
            new NpgsqlConnectionStringBuilder(
                builder.Configuration["Db:ConnectionString"])
            {
                //IncludeErrorDetail = true,
                Password = builder.Configuration["Db:Password"]
            }
            .ConnectionString))
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options => options
            .TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                //ValidIssuer = "MyAuthServer",
                ValidateAudience = false,
                //ValidAudience = "MyAuthClient",
                ValidateLifetime = false,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Convert.FromBase64String(
                        builder.Configuration["JwtBearer:IssuerSigningKey"])),
                ValidateIssuerSigningKey = true,
            })
        .Services
    .AddAuthorization()
    .AddScoped<PasswordHasher<Player>>()
    .AddScoped<ValidationService>()
    .AddScoped<DbService>();
var app = builder.Build();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

#region Пользователь

app.MapPost("api/auth/signup", async (
    HttpRequest request,
    PasswordHasher<Player> passwordHasher,
    DbService dbService) =>
{
    const string Message = "Логин занят";

    try
    {
        string login = request.Form["login"];
        if (login == app.Configuration["Admin:Login"])
            return Results.BadRequest(new { Message });

        string password = request.Form["password"];
        string passwordHash = passwordHasher.HashPassword(null, password);

        var playerId = await dbService.InsertPlayerAsync(login, passwordHash);

        var jwt = new JwtSecurityToken(
            claims: new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, playerId.ToString(), ClaimValueTypes.Integer),
                new Claim(ClaimTypes.Role, "player")
            },
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(
                    Convert.FromBase64String(
                        app.Configuration["JwtBearer:IssuerSigningKey"])),
                SecurityAlgorithms.HmacSha256));

        return Results.Json(new { bearerToken = new JwtSecurityTokenHandler().WriteToken(jwt) });
    }
    catch (DbUpdateException)
    {
        return Results.BadRequest(new { Message });
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

app.MapPost("api/auth/signin", async (
    HttpRequest request,
    PasswordHasher<Player> passwordHasher,
    DbService dbService) =>
{
    const string Message = "Логин или пароль неверен";

    try
    {
        string login = request.Form["login"];
        string password = request.Form["password"];
        bool isAdmin;
        var claims = new List<Claim>();

        if (login == app.Configuration["Admin:Login"])
        {
            if (password != app.Configuration["Admin:Password"])
                return Results.BadRequest(new { Message });

            isAdmin = true;
        }
        else
        {
            var player = await dbService.SelectPlayerAsync(login);

            if (player == null
                || passwordHasher.VerifyHashedPassword(
                    null,
                    player.PasswordHash,
                    password)
                == PasswordVerificationResult.Failed)
                return Results.BadRequest(new { Message });

            isAdmin = false;
            claims.Add(new Claim(
                ClaimTypes.NameIdentifier,
                player.PlayerId.ToString(),
                ClaimValueTypes.Integer));
        }
        claims.Add(new Claim(ClaimTypes.Role, isAdmin ? "admin" : "player"));

        var jwt = new JwtSecurityToken(
            claims: claims,
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(
                    Convert.FromBase64String(
                        app.Configuration["JwtBearer:IssuerSigningKey"])),
                SecurityAlgorithms.HmacSha256));

        return Results.Json(new
        {
            isAdmin,
            bearerToken = new JwtSecurityTokenHandler().WriteToken(jwt)
        },
        new System.Text.Json.JsonSerializerOptions
        {
            DefaultIgnoreCondition =
                System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault
        });
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

#endregion

#region Администратор - Словари

app.MapGet("api/dictionaries", [Authorize(Roles = "admin")] async (
    CrosswordsContext db) =>
{
    return Results.Json(await db.Dictionaries
        .Select(d => new
        {
            id = d.DictionaryId,
            name = d.DictionaryName
        })
        .ToListAsync());
});

app.MapPost("api/dictionaries", [Authorize(Roles = "admin")] async (
    HttpRequest request,
    DbService dbService,
    CrosswordsContext db) =>
{
    try
    {
        string name = request.Form["name"];
        var dictionaryFile = request.Form.Files["dictionary"];

        var id = await dbService.InsertDictionaryAsync(name, dictionaryFile);

        return Results.Json(new { id }, statusCode: 201);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { ex.Message });
    }
    catch (DbUpdateException ex)
    {
        string constraintName = (ex.InnerException as PostgresException).ConstraintName ?? "";

        string message = db.Dictionaries.EntityType.FindIndex(constraintName) is not null
            ? "Название занято"
            : db.Words.EntityType.FindIndex(constraintName) is not null
                ? "Слова неуникальны"
                : ex.Message;

        return Results.BadRequest(new { message });
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

app.MapPatch("api/dictionaries/{id}", [Authorize(Roles = "admin")] async (
    short id,
    HttpRequest request,
    DbService dbService) =>
{
    try
    {
        string name = request.Form["name"];

        await dbService.UpdateDictionaryAsync(id, name);

        return Results.NoContent();
    }
    catch (DbUpdateConcurrencyException)
    {
        return Results.BadRequest(new { message = "Словарь не найден" });
    }
    catch (DbUpdateException)
    {
        return Results.BadRequest(new { message = "Название занято" });
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

app.MapDelete("api/dictionaries/{id}", [Authorize(Roles = "admin")] async (
    short id,
    CrosswordsContext db) =>
{
    try
    {
        db.Dictionaries.Remove(new Dictionary
        {
            DictionaryId = id
        });
        await db.SaveChangesAsync();

        return Results.NoContent();
    }
    catch (DbUpdateConcurrencyException)
    {
        return Results.BadRequest(new { message = "Словарь не найден" });
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

#endregion

app.MapGet("api/user", [Authorize] () => "User");
app.MapGet("api/admin", [Authorize(Roles = "admin")] () => "Admin");
app.MapGet("api/player", [Authorize(Roles = "player")] () => "Player");

app.Run();
