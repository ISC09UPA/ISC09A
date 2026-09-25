using System.Net;
using System.Net.Http.Json;
using System.Text;
using ImageCards.Api.DTOs;

namespace ImageCards.Api.Tests.Api;

public sealed class ReviewsApiTests : ApiTestBase
{
    private Task<HttpResponseMessage> PostReviewJson(string json) =>
        Client.PostAsync("/api/reviews", new StringContent(json, Encoding.UTF8, "application/json"));

    [Fact]
    public async Task GetDue_ReturnsFlashcardShapeExpectedByMobileApp()
    {
        var card = await CreateCardAsync(("en", "Apple", "I eat an apple every day."));

        var response = await Client.GetAsync("/api/reviews/due?language=en");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await ReadJsonAsync(response);
        Assert.Equal(1, json.GetProperty("totalDue").GetInt32());
        var flashcard = json.GetProperty("cards")[0];
        Assert.Equal(card.Id, flashcard.GetProperty("id").GetGuid());
        Assert.StartsWith("https://", flashcard.GetProperty("imageUrl").GetString());
        Assert.Equal("Apple", flashcard.GetProperty("translation").GetString());
        Assert.Equal("en", flashcard.GetProperty("language").GetString());
        Assert.Equal("I eat an apple every day.", flashcard.GetProperty("exampleSentence").GetString());
        Assert.EndsWith("Z", flashcard.GetProperty("nextReviewAt").GetString());
    }

    [Fact]
    public async Task GetDue_WithoutLanguage_DefaultsToEnglish()
    {
        await CreateCardAsync(("en", "Apple", null));

        var due = await Client.GetFromJsonAsync<DueCardsResponseDto>("/api/reviews/due", Json);

        Assert.Equal("en", Assert.Single(due!.Cards).Language);
    }

    [Theory]
    [InlineData("/api/reviews/due?language=xx")]
    [InlineData("/api/reviews/due?limit=0")]
    [InlineData("/api/reviews/due?limit=500")]
    public async Task GetDue_WithInvalidQuery_Returns400(string url)
    {
        var response = await Client.GetAsync(url);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostReview_SchedulesCardAndRemovesItFromDueList()
    {
        var card = await CreateCardAsync(("en", "Apple", null));

        var response = await PostReviewJson($$"""{ "cardId": "{{card.Id}}", "rating": "Good" }""");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var state = await response.Content.ReadFromJsonAsync<ReviewStateDto>(Json);
        Assert.Equal(1, state!.IntervalDays);
        var due = await Client.GetFromJsonAsync<DueCardsResponseDto>("/api/reviews/due?language=en", Json);
        Assert.Equal(0, due!.TotalDue);

        Factory.Time.Advance(TimeSpan.FromDays(1));
        due = await Client.GetFromJsonAsync<DueCardsResponseDto>("/api/reviews/due?language=en", Json);
        Assert.Equal(1, due!.TotalDue);
    }

    [Fact]
    public async Task GetReviewState_ReturnsStateAfterReview()
    {
        var card = await CreateCardAsync(("en", "Apple", null));
        await PostReviewJson($$"""{ "cardId": "{{card.Id}}", "rating": "Easy" }""");

        var state = await Client.GetFromJsonAsync<ReviewStateDto>($"/api/reviews/{card.Id}", Json);

        Assert.Equal(4, state!.IntervalDays);
        Assert.Equal(1, state.Repetitions);
    }

    [Theory]
    [InlineData("""{ "cardId": "00000000-0000-0000-0000-000000000001", "rating": "Perfect" }""")]
    [InlineData("""{ "cardId": "00000000-0000-0000-0000-000000000001", "rating": 3 }""")]
    [InlineData("""{ "cardId": "00000000-0000-0000-0000-000000000001" }""")]
    [InlineData("""{ "rating": "Good" }""")]
    [InlineData("""{ "cardId": "not-a-guid", "rating": "Good" }""")]
    public async Task PostReview_WithInvalidBody_Returns400(string body)
    {
        var response = await PostReviewJson(body);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostReview_ForUnknownCard_Returns404()
    {
        var response = await PostReviewJson($$"""{ "cardId": "{{Guid.NewGuid()}}", "rating": "Good" }""");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
