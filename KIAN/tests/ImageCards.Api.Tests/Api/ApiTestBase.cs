using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using ImageCards.Api.DTOs;
using ImageCards.Api.Tests.TestSupport;

namespace ImageCards.Api.Tests.Api;

public abstract class ApiTestBase : IDisposable
{
    protected static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    protected ApiTestBase()
    {
        Factory = new ImageCardsApiFactory();
        Client = Factory.CreateClient();
    }

    protected ImageCardsApiFactory Factory { get; }

    protected HttpClient Client { get; }

    public void Dispose()
    {
        Client.Dispose();
        Factory.Dispose();
        GC.SuppressFinalize(this);
    }

    protected static MultipartFormDataContent CardForm(
        byte[]? image, string fileName = "apple.png", string contentType = "image/png",
        params (string Language, string Text, string? Example)[] translations)
    {
        var form = new MultipartFormDataContent();
        if (image is not null)
        {
            var file = new ByteArrayContent(image);
            file.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
            form.Add(file, "image", fileName);
        }

        for (var i = 0; i < translations.Length; i++)
        {
            form.Add(new StringContent(translations[i].Language), $"translations[{i}].language");
            form.Add(new StringContent(translations[i].Text), $"translations[{i}].translatedText");
            if (translations[i].Example is { } example)
            {
                form.Add(new StringContent(example), $"translations[{i}].exampleSentence");
            }
        }

        return form;
    }

    protected async Task<CardResponseDto> CreateCardAsync(params (string Language, string Text, string? Example)[] translations)
    {
        var response = await Client.PostAsync("/api/cards", CardForm(TestImages.Png, translations: translations));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<CardResponseDto>(Json))!;
    }

    protected static async Task<JsonElement> ReadJsonAsync(HttpResponseMessage response) =>
        await response.Content.ReadFromJsonAsync<JsonElement>(Json);
}
