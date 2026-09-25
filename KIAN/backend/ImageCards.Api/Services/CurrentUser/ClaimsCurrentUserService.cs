using System.Security.Claims;
using ImageCards.Api.Exceptions;

namespace ImageCards.Api.Services.CurrentUser;

/// <summary>
/// Reads the user id from the authenticated principal. Used outside Development.
/// No authentication scheme is configured yet, so every request is rejected with 401 until
/// JWT / Entra ID is wired up. This is intentional: there is no insecure fallback.
/// </summary>
public sealed class ClaimsCurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    // "oid" is the Entra ID object id; "sub" is the standard JWT subject.
    private static readonly string[] UserIdClaimTypes = ["oid", ClaimTypes.NameIdentifier, "sub"];

    public Guid UserId
    {
        get
        {
            var principal = httpContextAccessor.HttpContext?.User;
            if (principal?.Identity?.IsAuthenticated != true)
            {
                throw new UnauthenticatedException();
            }

            foreach (var claimType in UserIdClaimTypes)
            {
                if (Guid.TryParse(principal.FindFirstValue(claimType), out var userId))
                {
                    return userId;
                }
            }

            throw new UnauthenticatedException();
        }
    }
}
