using ImageCards.Api.Data;
using ImageCards.Api.Services.CurrentUser;
using ImageCards.Api.Services.Storage;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Time.Testing;

namespace ImageCards.Api.Tests.TestSupport;

/// <summary>Runs the real API pipeline with SQLite, fake blob storage, a fake clock and a switchable user.</summary>
public sealed class ImageCardsApiFactory : WebApplicationFactory<Program>
{
    private readonly SqliteTestDatabase _database = new();

    public FakeBlobStorageService BlobStorage { get; } = new();

    public FakeTimeProvider Time { get; } = TestClock.Create();

    public TestCurrentUserService CurrentUser { get; } = new();

    public ImageCardsDbContext CreateDbContext() => _database.CreateContext();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        // Satisfies options validation; the real BlobStorageService is replaced below and never connects.
        builder.UseSetting("AzureStorage:ConnectionString", "UseDevelopmentStorage=true");

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<ImageCardsDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<ImageCardsDbContext>>();
            services.AddDbContext<ImageCardsDbContext>(o => o.UseSqlite(_database.Connection));

            services.RemoveAll<IBlobStorageService>();
            services.AddSingleton<IBlobStorageService>(BlobStorage);

            services.RemoveAll<ICurrentUserService>();
            services.AddSingleton<ICurrentUserService>(CurrentUser);

            services.RemoveAll<TimeProvider>();
            services.AddSingleton<TimeProvider>(Time);
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _database.Dispose();
        }
    }
}
