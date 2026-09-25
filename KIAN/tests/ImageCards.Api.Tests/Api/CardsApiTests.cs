using System.Net;
using System.Net.Http.Json;
using ImageCards.Api.DTOs;
using ImageCards.Api.Tests.TestSupport;

namespace ImageCards.Api.Tests.Api;

public sealed class CardsApiTests : ApiTestBase
{
    [Fact]
    public async Task PostCard_WithValidMultipart_Returns201WithLocationAndBody()
    {
        var response = await Client.PostAsync("/api/cards",
            CardForm(TestImages.Png, translations: [("en", "Apple", "I eat an apple."), ("es", "Manzana", null)]));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var card = await response.Content.ReadFromJsonAsync<CardResponseDto>(Json);
        Assert.NotNull(card);
        Assert.Equal($"/api/cards/{card.Id}", response.Headers.Location?.AbsolutePath);
        Assert.Contains("sig=", card.ImageUrl);
        Assert.Equal(
            [new TranslationDto("en", "Apple", "I eat an apple."), new TranslationDto("es", "Manzana", null)],
            card.Translations);
    }

    [Fact]
    public async Task PostCard_WithoutImage_Returns400ValidationProblem()
    {
        var response = await Client.PostAsync("/api/cards", CardForm(null, translations: [("en", "Apple", null)]));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var problem = await ReadJsonAsync(response);
        Assert.True(problem.GetProperty("errors").TryGetProperty("Image", out _));
    }

    [Fact]
    public async Task PostCard_WithoutTranslations_Returns400()
    {
        var response = await Client.PostAsync("/api/cards", CardForm(TestImages.Png));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("xx")]
    [InlineData("english")]
    public async Task PostCard_WithUnsupportedLanguage_Returns400(string language)
    {
        var response = await Client.PostAsync("/api/cards", CardForm(TestImages.Png, translations: [(language, "Apple", null)]));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(Factory.BlobStorage.StoredBlobNames);
    }

    [Fact]
    public async Task PostCard_WithDuplicatedLanguages_Returns400()
    {
        var response = await Client.PostAsync("/api/cards",
            CardForm(TestImages.Png, translations: [("en", "Apple", null), ("EN", "Apple again", null)]));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await ReadJsonAsync(response);
        Assert.Contains("Duplicated", problem.GetProperty("errors").GetProperty("Translations")[0].GetString());
    }

    [Fact]
    public async Task PostCard_WithFakeImage_Returns400AndDoesNotUpload()
    {
        var response = await Client.PostAsync("/api/cards",
            CardForm(TestImages.NotAnImage, "shell.png", "image/png", ("en", "Apple", null)));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await ReadJsonAsync(response);
        Assert.True(problem.GetProperty("errors").TryGetProperty("image", out _));
        Assert.Empty(Factory.BlobStorage.StoredBlobNames);
    }

    [Fact]
    public async Task PostCard_WhenStorageIsDown_Returns503WithoutInternalDetails()
    {
        Factory.BlobStorage.FailUploads = true;

        var response = await Client.PostAsync("/api/cards", CardForm(TestImages.Png, translations: [("en", "Apple", null)]));

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("   at ", body); // no stack trace
        Assert.Contains("traceId", body);
    }

    [Fact]
    public async Task GetCard_ReturnsCreatedCard()
    {
        var created = await CreateCardAsync(("en", "Apple", null));

        var card = await Client.GetFromJsonAsync<CardResponseDto>($"/api/cards/{created.Id}", Json);

        Assert.Equal(created.Id, card!.Id);
    }

    [Fact]
    public async Task GetCard_WithLanguage_ReturnsOnlyThatTranslation()
    {
        var created = await CreateCardAsync(("en", "Apple", null), ("it", "Mela", null));

        var card = await Client.GetFromJsonAsync<CardResponseDto>($"/api/cards/{created.Id}?language=it", Json);

        Assert.Equal("Mela", Assert.Single(card!.Translations).TranslatedText);
    }

    [Fact]
    public async Task GetTranslation_ReturnsTranslation()
    {
        var created = await CreateCardAsync(("en", "Apple", null), ("pt", "Maçã", "Eu como uma maçã."));

        var translation = await Client.GetFromJsonAsync<TranslationDto>($"/api/cards/{created.Id}/translations/pt", Json);

        Assert.Equal(new TranslationDto("pt", "Maçã", "Eu como uma maçã."), translation);
    }

    [Fact]
    public async Task GetCard_Unknown_Returns404Problem()
    {
        var response = await Client.GetAsync($"/api/cards/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal(404, (await ReadJsonAsync(response)).GetProperty("status").GetInt32());
    }

    [Fact]
    public async Task GetCard_WithNonGuidId_Returns404()
    {
        var response = await Client.GetAsync("/api/cards/not-a-guid");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetCards_ReturnsPagedResultFilteredByLanguage()
    {
        await CreateCardAsync(("en", "Apple", null), ("fr", "Pomme", null));
        await CreateCardAsync(("en", "Dog", null));

        var page = await Client.GetFromJsonAsync<PagedResponse<CardResponseDto>>("/api/cards?language=fr", Json);

        Assert.Equal(1, page!.TotalCount);
        Assert.Equal("Pomme", Assert.Single(Assert.Single(page.Items).Translations).TranslatedText);
    }

    [Theory]
    [InlineData("/api/cards?language=xx")]
    [InlineData("/api/cards?pageSize=0")]
    [InlineData("/api/cards?pageSize=1000")]
    [InlineData("/api/cards?page=0")]
    public async Task GetCards_WithInvalidQuery_Returns400(string url)
    {
        var response = await Client.GetAsync(url);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PutCard_ReplacesTranslations()
    {
        var created = await CreateCardAsync(("en", "Apple", null));

        var response = await Client.PutAsync($"/api/cards/{created.Id}",
            CardForm(null, translations: [("en", "Red apple", null), ("de", "Apfel", null)]));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var card = await response.Content.ReadFromJsonAsync<CardResponseDto>(Json);
        Assert.Equal(["de", "en"], card!.Translations.Select(t => t.Language));
    }

    [Fact]
    public async Task PutCard_Unknown_Returns404()
    {
        var response = await Client.PutAsync($"/api/cards/{Guid.NewGuid()}", CardForm(null, translations: [("en", "x", null)]));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteCard_Returns204ThenCardIsGone()
    {
        var created = await CreateCardAsync(("en", "Apple", null));

        var delete = await Client.DeleteAsync($"/api/cards/{created.Id}");
        var get = await Client.GetAsync($"/api/cards/{created.Id}");

        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
        Assert.Empty(Factory.BlobStorage.StoredBlobNames);
    }

    [Fact]
    public async Task Cards_OfAnotherUser_AreNotVisible()
    {
        var created = await CreateCardAsync(("en", "Apple", null));
        Factory.CurrentUser.UserId = TestUsers.Bob;

        var get = await Client.GetAsync($"/api/cards/{created.Id}");
        var list = await Client.GetFromJsonAsync<PagedResponse<CardResponseDto>>("/api/cards", Json);

        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
        Assert.Empty(list!.Items);
    }
}
