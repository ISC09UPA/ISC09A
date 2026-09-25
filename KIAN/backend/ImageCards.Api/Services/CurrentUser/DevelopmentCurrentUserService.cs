using ImageCards.Api.Configuration;
using Microsoft.Extensions.Options;

namespace ImageCards.Api.Services.CurrentUser;

/// <summary>
/// DEVELOPMENT ONLY. Returns the fixed user configured in <c>DevelopmentUser</c>.
/// Registered exclusively in the Development environment (see <c>ServiceCollectionExtensions</c>).
/// </summary>
public sealed class DevelopmentCurrentUserService(IOptions<DevelopmentUserOptions> options) : ICurrentUserService
{
    public Guid UserId { get; } = options.Value.Id;
}
