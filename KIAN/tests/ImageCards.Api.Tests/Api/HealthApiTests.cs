using System.Net;

namespace ImageCards.Api.Tests.Api;

public sealed class HealthApiTests : ApiTestBase
{
    [Fact]
    public async Task Health_ReportsDatabaseAndStorageChecks()
    {
        var response = await Client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await ReadJsonAsync(response);
        Assert.Equal("Healthy", json.GetProperty("status").GetString());
        Assert.Equal("Healthy", json.GetProperty("checks").GetProperty("sql").GetString());
        Assert.Equal("Healthy", json.GetProperty("checks").GetProperty("blob-storage").GetString());
    }

    [Fact]
    public async Task Liveness_DoesNotRunDependencyChecks()
    {
        var response = await Client.GetAsync("/health/live");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await ReadJsonAsync(response);
        Assert.Empty(json.GetProperty("checks").EnumerateObject());
    }
}
