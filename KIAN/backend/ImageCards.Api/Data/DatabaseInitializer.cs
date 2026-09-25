using ImageCards.Api.Configuration;
using ImageCards.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ImageCards.Api.Data;

/// <summary>Optional startup tasks for local development. Nothing here runs in Production by default.</summary>
public static class DatabaseInitializer
{
    public const string ApplyMigrationsSetting = "Database:ApplyMigrationsOnStartup";

    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        var applyMigrations = app.Configuration.GetValue<bool>(ApplyMigrationsSetting);
        if (!applyMigrations && !app.Environment.IsDevelopment())
        {
            return;
        }

        await using var scope = app.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ImageCardsDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(DatabaseInitializer));

        try
        {
            if (applyMigrations)
            {
                logger.LogInformation("Applying database migrations");
                await db.Database.MigrateAsync();
            }

            if (app.Environment.IsDevelopment())
            {
                var user = scope.ServiceProvider.GetRequiredService<IOptions<DevelopmentUserOptions>>().Value;
                var time = scope.ServiceProvider.GetRequiredService<TimeProvider>();
                await EnsureDevelopmentUserAsync(db, user, time, logger);
            }
        }
        catch (Exception ex) when (app.Environment.IsDevelopment())
        {
            // Keep the API up (Swagger, /health) even if the local database is not ready yet.
            logger.LogWarning(ex, "Database initialization failed. Is SQL Server running and migrated?");
        }
    }

    private static async Task EnsureDevelopmentUserAsync(
        ImageCardsDbContext db, DevelopmentUserOptions options, TimeProvider time, ILogger logger)
    {
        if (await db.Users.AnyAsync(u => u.Id == options.Id))
        {
            return;
        }

        db.Users.Add(new User
        {
            Id = options.Id,
            Name = options.Name,
            Email = options.Email,
            CreatedAt = time.GetUtcNow().UtcDateTime,
        });
        await db.SaveChangesAsync();
        logger.LogInformation("Created development user {UserId}", options.Id);
    }
}
