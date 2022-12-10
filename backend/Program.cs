using Crosswords.Db;
using Crosswords.Db.Models;
using Crosswords.Models;
using Crosswords.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.RegularExpressions;

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
    .AddScoped<DbService>()
    .AddCors();
var app = builder.Build();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseCors(builder => builder.AllowAnyOrigin());

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

        return Results.Json(new { id }, statusCode: StatusCodes.Status201Created);
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

#region Администратор - Слова словаря

app.MapGet("api/dictionaries/{dictionaryId}/words", [Authorize(Roles = "admin")] async (
    short dictionaryId,
    string? mask,
    string? search,
    string sort,
    string? lastName,
    int limit,
    CrosswordsContext db) =>
{
    try
    {
        var words = db.Words
            .Where(w => w.DictionaryId == dictionaryId);

        if (mask is not null)
        {
            mask = $"^{mask.ToUpperInvariant()}$";

            words = words.Where(w => Regex.IsMatch(w.WordName, mask));
        }

        if (search is not null)
        {
            search = search.ToUpperInvariant();

            words = words.Where(w => w.WordName.Contains(search));
        }

        words = sort switch
        {
            "ascAlphabet" => words.OrderBy(w => w.WordName),
            "descAlphabet" => words.OrderByDescending(w => w.WordName),
            "ascLength" => words.OrderBy(w => w.WordName.Length).ThenBy(w => w.WordName),
            "descLength" => words.OrderByDescending(w => w.WordName.Length).ThenBy(w => w.WordName),
            _ => throw new NotImplementedException()
        };

        if (lastName is not null)
        {
            lastName = lastName.ToUpperInvariant();

            words = sort switch
            {
                "ascAlphabet" => words.Where(w => w.WordName.CompareTo(lastName) > 0),
                "descAlphabet" => words.Where(w => w.WordName.CompareTo(lastName) < 0),
                "ascLength" => words.Where(w => w.WordName.Length == lastName.Length && w.WordName.CompareTo(lastName) > 0 || w.WordName.Length > lastName.Length),
                "descLength" => words.Where(w => w.WordName.Length == lastName.Length && w.WordName.CompareTo(lastName) > 0 || w.WordName.Length < lastName.Length),
                _ => throw new NotImplementedException()
            };
        }

        return Results.Json(await words
            .Take(limit)
            .Select(w => new
            {
                id = w.WordId,
                name = w.WordName,
                definition = w.Definition
            })
            .ToListAsync());
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

app.MapPost("api/dictionaries/{dictionaryId}/words", [Authorize(Roles = "admin")] async (
    short dictionaryId,
    HttpRequest request,
    DbService dbService) =>
{
    try
    {
        string name = request.Form["name"];
        string definition = request.Form["definition"];

        var id = await dbService.InsertWordAsync(dictionaryId, name, definition);

        return Results.Json(new { id }, statusCode: StatusCodes.Status201Created);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { ex.Message });
    }
    catch (DbUpdateException ex)
    {
        string message = (ex.InnerException as PostgresException).SqlState switch
        {
            "23503" => "Словарь не найден",
            "23505" => "Название занято"
        };

        return Results.BadRequest(new { message });
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

app.MapPatch("api/words/{id}", [Authorize(Roles = "admin")] async (
    int id,
    HttpRequest request,
    DbService dbService) =>
{
    try
    {
        string definition = request.Form["definition"];

        await dbService.UpdateWordAsync(id, definition);

        return Results.NoContent();
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { ex.Message });
    }
    catch (DbUpdateException)
    {
        return Results.BadRequest(new { message = "Слово не найдено" });
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

app.MapDelete("api/words/{id}", [Authorize(Roles = "admin")] async (
    int id,
    CrosswordsContext db) =>
{
    try
    {
        db.Words.Remove(new Word
        {
            WordId = id
        });
        await db.SaveChangesAsync();

        return Results.NoContent();
    }
    catch (DbUpdateConcurrencyException)
    {
        return Results.BadRequest(new { message = "Слово не найдено" });
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

#endregion

#region Администратор - Темы

app.MapGet("api/themes", [Authorize(Roles = "admin")] async (
    CrosswordsContext db) =>
{
    return Results.Json(await db.Themes
        .Select(t => new
        {
            id = t.ThemeId,
            name = t.ThemeName
        })
        .ToListAsync());
});

app.MapPost("api/themes", [Authorize(Roles = "admin")] async (
    HttpRequest request,
    DbService dbService) =>
{
    try
    {
        string name = request.Form["name"];

        var id = await dbService.InsertThemeAsync(name);

        return Results.Json(new { id }, statusCode: StatusCodes.Status201Created);
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

app.MapPut("api/themes/{id}", [Authorize(Roles = "admin")] async (
    short id,
    HttpRequest request,
    DbService dbService) =>
{
    try
    {
        string name = request.Form["name"];

        await dbService.UpdateThemeAsync(id, name);

        return Results.NoContent();
    }
    catch (DbUpdateConcurrencyException)
    {
        return Results.BadRequest(new { message = "Тема не найдена" });
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

app.MapDelete("api/themes/{id}", [Authorize(Roles = "admin")] async (
    short id,
    CrosswordsContext db) =>
{
    try
    {
        db.Themes.Remove(new Theme
        {
            ThemeId = id
        });
        await db.SaveChangesAsync();

        return Results.NoContent();
    }
    catch (DbUpdateConcurrencyException)
    {
        return Results.BadRequest(new { message = "Тема не найдена" });
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

#endregion

#region Администратор - Кроссворды

app.MapGet("api/themes/{themeId}/crosswords", [Authorize(Roles = "admin")] async (
    short themeId,
    CrosswordsContext db) =>
{
    return Results.Json(await db.Crosswords
        .Where(c => c.ThemeId == themeId)
        .Select(c => new
        {
            id = c.CrosswordId,
            name = c.CrosswordName
        })
        .ToListAsync());
});

app.MapGet("api/crosswords/{id}", [Authorize(Roles = "admin")] async (
    short id,
    CrosswordsContext db) =>
{
    return Results.Json(await db.Crosswords
        .Where(c => c.CrosswordId == id)
        .Select(c => new
        {
            c.DictionaryId,
            size = new
            {
                width = c.HorizontalSize,
                height = c.VerticalSize
            },
            c.PromptCount,
            words = c.CrosswordWords
                .Select(cw => new
                {
                    id = cw.WordId,
                    name = cw.Word.WordName,
                    definition = cw.Word.Definition,
                    p1 = new
                    {
                        x = cw.X1,
                        y = cw.Y1
                    },
                    p2 = new
                    {
                        x = cw.X2,
                        y = cw.Y2
                    }
                })
        })
        .SingleOrDefaultAsync());
});

app.MapPost("api/crosswords", [Authorize(Roles = "admin")] async (
    HttpRequest request,
    DbService dbService) =>
{
    try
    {
        var crossword = await request.ReadFromJsonAsync<CrosswordModel>();

        var id = await dbService.InsertCrosswordAsync(crossword);

        return Results.Json(new { id }, statusCode: StatusCodes.Status201Created);
    }
    catch (DbUpdateException ex)
    {
        string message = ex.InnerException is not PostgresException postgresException
            ? ex.Message
            : postgresException.SqlState switch
            {
                "23503" => postgresException.ConstraintName is null
                    ? ex.Message
                    : postgresException.ConstraintName.Contains("theme_id")
                        ? "Тема не найдена"
                        : postgresException.ConstraintName.Contains("dictionary_id")
                            ? "Словарь не найден"
                            : postgresException.ConstraintName.Contains("word_id")
                                ? "Слово не найдено"
                                : ex.Message,
                "23505" => "Название занято",
                _ => ex.Message
            };

        return Results.BadRequest(new { message });
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

app.MapPut("api/crosswords/{id}", [Authorize(Roles = "admin")] async (
    short id,
    HttpRequest request,
    DbService dbService) =>
{
    try
    {
        var crossword = await request.ReadFromJsonAsync<CrosswordModel>();

        await dbService.UpdateCrosswordAsync(id, crossword);

        return Results.NoContent();
    }
    catch (InvalidOperationException)
    {
        return Results.BadRequest(new { message = "Кроссворд не найден" });
    }
    catch (DbUpdateException ex)
    {
        string message = ex.InnerException is not PostgresException postgresException
            ? ex.Message
            : postgresException.SqlState switch
            {
                "23503" => postgresException.ConstraintName is null
                    ? ex.Message
                    : postgresException.ConstraintName.Contains("theme_id")
                        ? "Тема не найдена"
                        : postgresException.ConstraintName.Contains("dictionary_id")
                            ? "Словарь не найден"
                            : postgresException.ConstraintName.Contains("word_id")
                                ? "Слово не найдено"
                                : ex.Message,
                "23505" => "Название занято",
                _ => ex.Message
            };

        return Results.BadRequest(new { message });
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

app.MapDelete("api/crosswords/{id}", [Authorize(Roles = "admin")] async (
    short id,
    CrosswordsContext db) =>
{
    try
    {
        db.Crosswords.Remove(new Crossword
        {
            CrosswordId = id
        });
        await db.SaveChangesAsync();

        return Results.NoContent();
    }
    catch (DbUpdateConcurrencyException)
    {
        return Results.BadRequest(new { message = "Кроссворд не найден" });
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
