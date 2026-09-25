using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ImageCards.Api.HealthChecks;

public static class HealthCheckEndpoints
{
    public const string HealthPath = "/health";
    public const string LivenessPath = "/health/live";

    /// <summary>
    /// <c>/health</c>: checks SQL and Blob Storage (use it as the Azure App Service health check path).
    /// <c>/health/live</c>: process only, no dependencies.
    /// </summary>
    public static WebApplication MapImageCardsHealthChecks(this WebApplication app)
    {
        app.MapHealthChecks(HealthPath, new HealthCheckOptions { ResponseWriter = WriteResponseAsync });
        app.MapHealthChecks(LivenessPath, new HealthCheckOptions { Predicate = _ => false, ResponseWriter = WriteResponseAsync });
        return app;
    }

    // Only names and statuses: exception details could reveal connection information.
    private static Task WriteResponseAsync(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";
        var body = new
        {
            status = report.Status.ToString(),
            totalDurationMs = (int)report.TotalDuration.TotalMilliseconds,
            checks = report.Entries.ToDictionary(e => e.Key, e => e.Value.Status.ToString()),
        };
        return context.Response.WriteAsync(JsonSerializer.Serialize(body, JsonSerializerOptions.Web));
    }
}
