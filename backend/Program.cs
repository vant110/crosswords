using Crosswords.Db;
using Crosswords.Db.Models;
using Crosswords.Models;
using Crosswords.Models.DTOs;
using Crosswords.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
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
    .AddScoped<DbService>()
    .AddScoped<FileService>()
    .AddScoped<ValidationService>()
    .AddCors();
var app = builder.Build();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseCors(builder => builder.AllowAnyOrigin());

#region Пользователь

app.MapPost("api/auth/signup", async (
    UserDTO user,
    PasswordHasher<Player> passwordHasher,
    DbService dbService) =>
{
    const string Message = "Логин занят";

    try
    {
        if (user.Login == app.Configuration["Admin:Login"])
            return Results.BadRequest(new { Message });

        string passwordHash = passwordHasher.HashPassword(null, user.Password);

        var playerId = await dbService.InsertPlayerAsync(user.Login, passwordHash);

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
    UserDTO user,
    PasswordHasher<Player> passwordHasher,
    DbService dbService) =>
{
    const string Message = "Логин или пароль неверен";

    try
    {
        bool isAdmin;
        var claims = new List<Claim>();

        if (user.Login == app.Configuration["Admin:Login"])
        {
            if (user.Password != app.Configuration["Admin:Password"])
                return Results.BadRequest(new { Message });

            isAdmin = true;
        }
        else
        {
            var player = await dbService.SelectPlayerAsync(user.Login);

            if (player == null
                || passwordHasher.VerifyHashedPassword(
                    null,
                    player.PasswordHash,
                    user.Password)
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
        .OrderBy(d => d.DictionaryName)
        .Select(d => new
        {
            id = d.DictionaryId,
            name = d.DictionaryName
        })
        .ToListAsync());
});

app.MapPost("api/dictionaries", [Authorize(Roles = "admin")] async (
    HttpRequest request,
    FileService fileService,
    DbService dbService) =>
{
    try
    {
        string name = request.Form["name"];
        var dictionaryFile = request.Form.Files["dictionary"];

        string encoding = "utf-8";
        if (request.Form.TryGetValue("encoding", out var encodings)
            && encodings[0] is not null)
        {
            encoding = encodings[0];
        }

        string separator = " ";
        if (request.Form.TryGetValue("separator", out var separators)
            && separators[0] is not null)
        {
            separator = separators[0];
        }

        bool skipInvalid = request.Form.ContainsKey("skipInvalid");

        var words = await fileService.ReadWordsAsync(dictionaryFile, encoding, separator, skipInvalid);
        var id = await dbService.InsertDictionaryAsync(name, words);

        return Results.Json(new { id }, statusCode: StatusCodes.Status201Created);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { ex.Message });
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

app.MapPatch("api/dictionaries/{id}", [Authorize(Roles = "admin")] async (
    short id,
    DictionaryDTO dictionary,
    DbService dbService) =>
{
    try
    {
        await dbService.UpdateDictionaryAsync(id, dictionary.Name);

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
            search = search.ToUpper();

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
            lastName = lastName.ToUpper();

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
    WordDTO word,
    DbService dbService) =>
{
    try
    {
        var id = await dbService.InsertWordAsync(dictionaryId, word.Name, word.Definition);

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
    WordDTO word,
    DbService dbService) =>
{
    try
    {
        await dbService.UpdateWordAsync(id, word.Definition);

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

app.MapGet("api/themes", [Authorize(Roles = "admin, player")] async (
    CrosswordsContext db) =>
{
    return Results.Json(await db.Themes
        .OrderBy(t => t.ThemeName)
        .Select(t => new
        {
            id = t.ThemeId,
            name = t.ThemeName
        })
        .ToListAsync());
});

app.MapPost("api/themes", [Authorize(Roles = "admin")] async (
    ThemeDTO theme,
    DbService dbService) =>
{
    try
    {
        var id = await dbService.InsertThemeAsync(theme.Name);

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
    ThemeDTO theme,
    DbService dbService) =>
{
    try
    {
        await dbService.UpdateThemeAsync(id, theme.Name);

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
        .OrderBy(c => c.CrosswordName)
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
                c.Width,
                c.Height
            },
            c.PromptCount,
            words = c.CrosswordWords
                .OrderBy(cw => cw.Word.WordName)
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
    CrosswordDTO crossword,
    DbService dbService) =>
{
    try
    {
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
    CrosswordDTO crossword,
    DbService dbService) =>
{
    try
    {
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

app.MapGet("api/dictionaries/{dictionaryId}/generate_crossword", [Authorize(Roles = "admin")] async (
    short dictionaryId,
    int width,
    int height,
    CrosswordsContext db) =>
{
    try
    {
        int maxWordLength = width > height
            ? width
            : height;

        var words = await db.Words
            .Where(w => w.DictionaryId == dictionaryId
                && w.WordName.Length <= maxWordLength)
            .OrderByDescending(w => w.WordName.Length)
            .Select(w => new Word
            {
                WordId = w.WordId,
                WordName = w.WordName,
                Definition = w.Definition
            })
            .ToListAsync();

        var grid = new GridModel(width, height);

        grid.GenerateCrossword(words);

        if (app.Environment.IsDevelopment())
        {
            var stringBuilder = new StringBuilder();
            for (int y = 0; y < grid.Height; y++)
            {
                for (int x = 0; x < grid.Width; x++)
                {
                    stringBuilder.Append(grid.Cells[x, y]);
                }
                if (y != grid.Height - 1)
                {
                    stringBuilder.AppendLine();
                }
            }
            app.Logger.LogInformation(stringBuilder.ToString());
        }

        return Results.Json(grid.ToCrosswordWordDTOs());
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { ex.Message });
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

#endregion

#region Игрок - Кроссворды

app.MapGet("api/themes/{themeId}/crosswords/unstarted", [Authorize(Roles = "player")] async (
    short themeId,
    HttpContext context,
    CrosswordsContext db) =>
{
    int playerId = int.Parse(context.User.FindFirst(ClaimTypes.NameIdentifier).Value);

    return Results.Json(await db.Crosswords
        .Where(c => c.ThemeId == themeId
            && !c.Saves
                .Where(s => s.PlayerId == playerId)
                .Any()
            && !c.SolvedCrosswords
                .Where(sc => sc.PlayerId == playerId)
                .Any())
        .OrderBy(c => c.CrosswordName)
        .Select(c => new
        {
            id = c.CrosswordId,
            name = c.CrosswordName
        })
        .ToListAsync());
});

app.MapGet("api/themes/{themeId}/crosswords/started", [Authorize(Roles = "player")] async (
    short themeId,
    HttpContext context,
    CrosswordsContext db) =>
{
    int playerId = int.Parse(context.User.FindFirst(ClaimTypes.NameIdentifier).Value);

    return Results.Json(await db.Crosswords
        .Where(c => c.ThemeId == themeId
            && c.Saves
                .Where(p => p.PlayerId == playerId)
                .Any())
        .OrderBy(c => c.CrosswordName)
        .Select(c => new
        {
            id = c.CrosswordId,
            name = c.CrosswordName
        })
        .ToListAsync());
});


#endregion

app.MapGet("api/user", [Authorize] () => "User");
app.MapGet("api/admin", [Authorize(Roles = "admin")] () => "Admin");
app.MapGet("api/player", [Authorize(Roles = "player")] () => "Player");

app.Run();
