using ImageCards.Api.Data;
using ImageCards.Api.Models;
using ImageCards.Api.Services.CurrentUser;

namespace ImageCards.Api.Tests.TestSupport;

public static class TestUsers
{
    public static readonly Guid Alice = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid Bob = Guid.Parse("22222222-2222-2222-2222-222222222222");

    public static void Seed(ImageCardsDbContext db, DateTime createdAt)
    {
        db.Users.AddRange(
            new User { Id = Alice, Name = "Alice", Email = "alice@test.local", CreatedAt = createdAt },
            new User { Id = Bob, Name = "Bob", Email = "bob@test.local", CreatedAt = createdAt });
        db.SaveChanges();
    }
}

/// <summary>Current user whose id tests can switch.</summary>
public sealed class TestCurrentUserService : ICurrentUserService
{
    public Guid UserId { get; set; } = TestUsers.Alice;
}
