using Crosswords.Db;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services
    .AddDbContextPool<CrosswordsContext>(options => options
        //.EnableThreadSafetyChecks(false)
        .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
        .UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
var app = builder.Build();
app.UseDefaultFiles();
app.UseStaticFiles();


app.MapGet("/test", async (
    CrosswordsContext db) =>
{
    try
    {
        return Results.Json(await db.Themes
            .Select(t => t)
            .ToListAsync());
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex.ToString());
        return Results.Problem(ex.ToString());
    }
});


app.Run();
