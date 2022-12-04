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
                IncludeErrorDetail = true,
                Password = builder.Configuration["Db:Password"]
            }
            .ConnectionString))
    .AddScoped<DbService>()
    .AddScoped<PasswordHasher<Player>>()
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
    .AddAuthorization();
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

        int playerId = await dbService.InsertPlayerAsync(login, passwordHash);

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

app.MapGet("api/user", [Authorize] () => "User");
app.MapGet("api/admin", [Authorize(Roles = "admin")] () => "Admin");
app.MapGet("api/player", [Authorize(Roles = "player")] () => "Player");

app.Run();
