using ImageCards.Api.DTOs;
using ImageCards.Api.Exceptions;
using ImageCards.Api.Services.Storage;
using ImageCards.Api.Tests.TestSupport;
using Microsoft.EntityFrameworkCore;
using static ImageCards.Api.Tests.Services.ServiceTestContext;

namespace ImageCards.Api.Tests.Services;

public sealed class CardServiceTests : IDisposable
{
    private readonly ServiceTestContext _ctx = new();

    public void Dispose() => _ctx.Dispose();

    [Fact]
    public async Task CreateCard_StoresImageInBlobStorageAndOnlyItsNameInDatabase()
    {
        var card = await _ctx.CreateCardAsync(Translation("en", "Apple", "I eat an apple."));

        var blobName = Assert.Single(_ctx.BlobStorage.StoredBlobNames);
        Assert.True(BlobNames.IsValid(blobName));
        Assert.Equal("image/png", _ctx.BlobStorage.ContentTypeOf(blobName));
        Assert.Contains(blobName, card.ImageUrl);

        var stored = await _ctx.NewDbContext().Cards.Include(c => c.Translations).SingleAsync();
        Assert.Equal(card.Id, stored.Id);
        Assert.Equal(TestUsers.Alice, stored.UserId);
        Assert.Equal(blobName, stored.ImageBlobName);
        Assert.Equal(TestClock.Start.UtcDateTime, stored.CreatedAt);
        var translation = Assert.Single(stored.Translations);
        Assert.Equal(("en", "Apple", "I eat an apple."), (translation.LanguageCode, translation.TranslatedText, translation.ExampleSentence));
    }

    [Fact]
    public async Task CreateCard_CreatesReviewStateDueImmediately()
    {
        var card = await _ctx.CreateCardAsync();

        var review = await _ctx.NewDbContext().CardReviews.SingleAsync();
        Assert.Equal(card.Id, review.CardId);
        Assert.Equal(TestUsers.Alice, review.UserId);
        Assert.Equal(TestClock.Start.UtcDateTime, review.NextReviewAt);
        Assert.Equal(0, review.Repetitions);
        Assert.Equal(2.5, review.EaseFactor);
    }

    [Fact]
    public async Task CreateCard_NormalizesLanguageAndTrimsText()
    {
        var card = await _ctx.CreateCardAsync(Translation(" ES ", "  Manzana  ", "   "));

        var translation = Assert.Single(card.Translations);
        Assert.Equal(new TranslationDto("es", "Manzana", null), translation);
    }

    [Fact]
    public async Task CreateCard_IgnoresClientFileNameForBlobName()
    {
        var image = TestImages.PngFile("../../../etc/passwd.png");

        await _ctx.CardService().CreateCardAsync(CreateRequest(image), default);

        var blobName = Assert.Single(_ctx.BlobStorage.StoredBlobNames);
        Assert.DoesNotContain("passwd", blobName);
        Assert.DoesNotContain("/", blobName);
    }

    [Fact]
    public async Task CreateCard_WithInvalidImage_ThrowsAndStoresNothing()
    {
        var image = TestImages.FormFile(TestImages.NotAnImage, "apple.png", "image/png");

        await Assert.ThrowsAsync<RequestValidationException>(
            () => _ctx.CardService().CreateCardAsync(CreateRequest(image), default));

        Assert.Empty(_ctx.BlobStorage.StoredBlobNames);
        Assert.Empty(await _ctx.NewDbContext().Cards.ToListAsync());
    }

    [Fact]
    public async Task CreateCard_WhenStorageFails_ThrowsAndStoresNothing()
    {
        _ctx.BlobStorage.FailUploads = true;

        await Assert.ThrowsAsync<StorageUnavailableException>(() => _ctx.CreateCardAsync());

        Assert.Empty(await _ctx.NewDbContext().Cards.ToListAsync());
    }

    [Fact]
    public async Task GetCard_ReturnsCardWithAllTranslationsSorted()
    {
        var created = await _ctx.CreateCardAsync(Translation("es", "Manzana"), Translation("en", "Apple"));

        var card = await _ctx.CardService().GetCardAsync(created.Id, language: null, default);

        Assert.Equal(created.Id, card.Id);
        Assert.StartsWith("https://", card.ImageUrl);
        Assert.Equal(["en", "es"], card.Translations.Select(t => t.Language));
    }

    [Fact]
    public async Task GetCard_WithLanguage_ReturnsOnlyThatTranslation()
    {
        var created = await _ctx.CreateCardAsync(Translation("es", "Manzana"), Translation("en", "Apple"));

        var card = await _ctx.CardService().GetCardAsync(created.Id, "es", default);

        Assert.Equal("Manzana", Assert.Single(card.Translations).TranslatedText);
    }

    [Fact]
    public async Task GetCard_WithMissingLanguage_ThrowsNotFound()
    {
        var created = await _ctx.CreateCardAsync(Translation("en", "Apple"));

        await Assert.ThrowsAsync<NotFoundException>(() => _ctx.CardService().GetCardAsync(created.Id, "fr", default));
    }

    [Fact]
    public async Task GetTranslation_ReturnsTranslationForLanguage()
    {
        var created = await _ctx.CreateCardAsync(Translation("en", "Apple"), Translation("de", "Apfel", "Ich esse einen Apfel."));

        var translation = await _ctx.CardService().GetTranslationAsync(created.Id, "de", default);

        Assert.Equal(new TranslationDto("de", "Apfel", "Ich esse einen Apfel."), translation);
    }

    [Fact]
    public async Task GetCard_ThatDoesNotExist_ThrowsNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _ctx.CardService().GetCardAsync(Guid.NewGuid(), null, default));
    }

    [Fact]
    public async Task GetCard_OwnedByAnotherUser_ThrowsNotFound()
    {
        var created = await _ctx.CreateCardAsync();
        _ctx.CurrentUser.UserId = TestUsers.Bob;

        await Assert.ThrowsAsync<NotFoundException>(() => _ctx.CardService().GetCardAsync(created.Id, null, default));
    }

    [Fact]
    public async Task GetCards_ReturnsOnlyCurrentUserCardsNewestFirstWithPaging()
    {
        var first = await _ctx.CreateCardAsync(Translation("en", "Apple"));
        _ctx.Time.Advance(TimeSpan.FromMinutes(1));
        var second = await _ctx.CreateCardAsync(Translation("en", "Pear"));
        _ctx.Time.Advance(TimeSpan.FromMinutes(1));
        var third = await _ctx.CreateCardAsync(Translation("en", "Plum"));
        _ctx.CurrentUser.UserId = TestUsers.Bob;
        await _ctx.CreateCardAsync(Translation("en", "Bob's card"));
        _ctx.CurrentUser.UserId = TestUsers.Alice;

        var page1 = await _ctx.CardService().GetCardsAsync(new CardQueryParameters { Page = 1, PageSize = 2 }, default);
        var page2 = await _ctx.CardService().GetCardsAsync(new CardQueryParameters { Page = 2, PageSize = 2 }, default);

        Assert.Equal(3, page1.TotalCount);
        Assert.Equal([third.Id, second.Id], page1.Items.Select(c => c.Id));
        Assert.Equal([first.Id], page2.Items.Select(c => c.Id));
    }

    [Fact]
    public async Task GetCards_WithLanguage_FiltersCardsAndTranslations()
    {
        await _ctx.CreateCardAsync(Translation("en", "Apple"), Translation("es", "Manzana"));
        await _ctx.CreateCardAsync(Translation("en", "Dog"));

        var result = await _ctx.CardService().GetCardsAsync(new CardQueryParameters { Language = "es" }, default);

        var card = Assert.Single(result.Items);
        Assert.Equal(1, result.TotalCount);
        Assert.Equal("Manzana", Assert.Single(card.Translations).TranslatedText);
    }

    [Fact]
    public async Task UpdateCard_ReplacesTranslationsAndKeepsImageWhenNoneSent()
    {
        var created = await _ctx.CreateCardAsync(Translation("en", "Apple"), Translation("es", "Manzana"));
        var originalBlob = Assert.Single(_ctx.BlobStorage.StoredBlobNames);

        var updated = await _ctx.CardService().UpdateCardAsync(created.Id, new UpdateCardRequestDto
        {
            Translations = [Translation("en", "Green apple", "A green apple."), Translation("fr", "Pomme")],
        }, default);

        Assert.Equal(
            [new TranslationDto("en", "Green apple", "A green apple."), new TranslationDto("fr", "Pomme", null)],
            updated.Translations);
        Assert.Equal([originalBlob], _ctx.BlobStorage.StoredBlobNames);
        var stored = await _ctx.NewDbContext().CardTranslations.OrderBy(t => t.LanguageCode).ToListAsync();
        Assert.Equal(["en", "fr"], stored.Select(t => t.LanguageCode));
    }

    [Fact]
    public async Task UpdateCard_WithNewImage_ReplacesBlobAndDeletesOldOne()
    {
        var created = await _ctx.CreateCardAsync();
        var originalBlob = Assert.Single(_ctx.BlobStorage.StoredBlobNames);

        await _ctx.CardService().UpdateCardAsync(created.Id, new UpdateCardRequestDto
        {
            Image = TestImages.FormFile(TestImages.Webp, "new.webp", "image/webp"),
            Translations = [Translation("en", "Apple")],
        }, default);

        var newBlob = Assert.Single(_ctx.BlobStorage.StoredBlobNames);
        Assert.NotEqual(originalBlob, newBlob);
        Assert.EndsWith(".webp", newBlob);
        Assert.Equal(newBlob, (await _ctx.NewDbContext().Cards.SingleAsync()).ImageBlobName);
    }

    [Fact]
    public async Task UpdateCard_ThatDoesNotExist_ThrowsNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _ctx.CardService().UpdateCardAsync(
            Guid.NewGuid(), new UpdateCardRequestDto { Translations = [Translation("en", "x")] }, default));
    }

    [Fact]
    public async Task DeleteCard_RemovesCardTranslationsReviewAndBlob()
    {
        var created = await _ctx.CreateCardAsync(Translation("en", "Apple"), Translation("es", "Manzana"));

        await _ctx.CardService().DeleteCardAsync(created.Id, default);

        var db = _ctx.NewDbContext();
        Assert.Empty(await db.Cards.ToListAsync());
        Assert.Empty(await db.CardTranslations.ToListAsync());
        Assert.Empty(await db.CardReviews.ToListAsync());
        Assert.Empty(_ctx.BlobStorage.StoredBlobNames);
    }

    [Fact]
    public async Task DeleteCard_WhenBlobDeletionFails_StillDeletesCard()
    {
        var created = await _ctx.CreateCardAsync();
        _ctx.BlobStorage.FailDeletes = true;

        await _ctx.CardService().DeleteCardAsync(created.Id, default);

        Assert.Empty(await _ctx.NewDbContext().Cards.ToListAsync());
    }

    [Fact]
    public async Task DeleteCard_ThatDoesNotExist_ThrowsNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _ctx.CardService().DeleteCardAsync(Guid.NewGuid(), default));
    }

    [Fact]
    public async Task DeleteCard_OwnedByAnotherUser_ThrowsNotFoundAndKeepsCard()
    {
        var created = await _ctx.CreateCardAsync();
        _ctx.CurrentUser.UserId = TestUsers.Bob;

        await Assert.ThrowsAsync<NotFoundException>(() => _ctx.CardService().DeleteCardAsync(created.Id, default));

        Assert.Single(await _ctx.NewDbContext().Cards.ToListAsync());
    }
}
