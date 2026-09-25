using ImageCards.Api.Configuration;
using ImageCards.Api.Data;
using ImageCards.Api.DTOs;
using ImageCards.Api.Services.Cards;
using ImageCards.Api.Services.Images;
using ImageCards.Api.Services.Reviews;
using ImageCards.Api.Services.SpacedRepetition;
using ImageCards.Api.Tests.TestSupport;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;

namespace ImageCards.Api.Tests.Services;

/// <summary>Real services over SQLite, with fake storage, clock and user. One per test.</summary>
public sealed class ServiceTestContext : IDisposable
{
    private readonly SqliteTestDatabase _database = new();
    private readonly List<ImageCardsDbContext> _contexts = [];

    public FakeBlobStorageService BlobStorage { get; } = new();

    public FakeTimeProvider Time { get; } = TestClock.Create();

    public TestCurrentUserService CurrentUser { get; } = new();

    public Sm2SpacedRepetitionScheduler Scheduler { get; } = new();

    public ImageCardsDbContext NewDbContext()
    {
        var db = _database.CreateContext();
        _contexts.Add(db);
        return db;
    }

    /// <summary>Each call uses a fresh DbContext, like a new HTTP request.</summary>
    public CardService CardService() => new(
        NewDbContext(),
        CurrentUser,
        BlobStorage,
        new ImageValidator(Options.Create(new ImageUploadOptions())),
        Scheduler,
        Time,
        NullLogger<CardService>.Instance);

    public ReviewService ReviewService() => new(
        NewDbContext(),
        CurrentUser,
        BlobStorage,
        Scheduler,
        Time,
        NullLogger<ReviewService>.Instance);

    public static CreateCardRequestDto CreateRequest(IFormFile? image = null, params TranslationRequestDto[] translations) =>
        new()
        {
            Image = image ?? TestImages.PngFile(),
            Translations = translations.Length > 0 ? [.. translations] : [Translation("en", "Apple", "I eat an apple.")],
        };

    public static TranslationRequestDto Translation(string language, string text, string? example = null) =>
        new() { Language = language, TranslatedText = text, ExampleSentence = example };

    public Task<CardResponseDto> CreateCardAsync(params TranslationRequestDto[] translations) =>
        CardService().CreateCardAsync(CreateRequest(null, translations), default);

    public void Dispose()
    {
        foreach (var db in _contexts)
        {
            db.Dispose();
        }

        _database.Dispose();
    }
}
