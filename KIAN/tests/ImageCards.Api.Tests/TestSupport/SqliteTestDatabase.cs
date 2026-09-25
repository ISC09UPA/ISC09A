using ImageCards.Api.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ImageCards.Api.Tests.TestSupport;

/// <summary>
/// In-memory SQLite database that lives as long as this object. Unlike the EF InMemory provider,
/// SQLite enforces foreign keys and unique indexes, so constraint bugs are caught in tests.
/// </summary>
public sealed class SqliteTestDatabase : IDisposable
{
    private readonly SqliteConnection _connection;

    public SqliteTestDatabase()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        using var db = CreateContext();
        db.Database.EnsureCreated();
        TestUsers.Seed(db, TestClock.Start.UtcDateTime);
    }

    public DbContextOptions<ImageCardsDbContext> Options =>
        new DbContextOptionsBuilder<ImageCardsDbContext>().UseSqlite(_connection).Options;

    public SqliteConnection Connection => _connection;

    public ImageCardsDbContext CreateContext() => new(Options);

    public void Dispose() => _connection.Dispose();
}
