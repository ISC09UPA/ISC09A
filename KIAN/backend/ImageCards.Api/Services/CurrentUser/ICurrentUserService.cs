namespace ImageCards.Api.Services.CurrentUser;

/// <summary>
/// Identifies the user making the request. The only place services get a user id from.
/// Swap the registered implementation to move to JWT / Entra ID without touching services or controllers.
/// </summary>
public interface ICurrentUserService
{
    Guid UserId { get; }
}
