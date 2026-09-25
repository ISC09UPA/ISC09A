using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ImageCards.Api.Data;

/// <summary>
/// Used only by the <c>dotnet ef</c> tools, so creating migrations does not need the app configuration.
/// <c>dotnet ef database update</c> takes the connection string from
/// <c>ConnectionStrings__DefaultConnection</c> or from the <c>--connection</c> option.
/// </summary>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ImageCardsDbContext>
{
    // No credentials: only used to generate migrations, never to connect to a real server.
    private const string PlaceholderConnectionString =
        "Server=localhost;Database=ImageCards;Integrated Security=true;TrustServerCertificate=true";

    public ImageCardsDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        var options = new DbContextOptionsBuilder<ImageCardsDbContext>()
            .UseSqlServer(string.IsNullOrWhiteSpace(connectionString) ? PlaceholderConnectionString : connectionString)
            .Options;
        return new ImageCardsDbContext(options);
    }
}
