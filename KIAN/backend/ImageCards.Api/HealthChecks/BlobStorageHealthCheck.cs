using ImageCards.Api.Services.Storage;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ImageCards.Api.HealthChecks;

public sealed class BlobStorageHealthCheck(IBlobStorageService blobStorage) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken = default) =>
        await blobStorage.CanConnectAsync(cancellationToken)
            ? HealthCheckResult.Healthy()
            : new HealthCheckResult(context.Registration.FailureStatus, "Blob storage is unreachable.");
}
