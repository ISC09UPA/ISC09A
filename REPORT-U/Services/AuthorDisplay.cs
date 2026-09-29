using ReportU.Dtos;
using ReportU.Models;

namespace ReportU.Services;

/// <summary>
/// Resuelve cómo se muestra un autor según el modo de identidad de la publicación
/// y la preferencia del perfil. El email nunca se incluye.
/// </summary>
public static class AuthorDisplay
{
    public const string AnonymousName = "Anónimo";

    public static AuthorInfo Resolve(User author, IdentityMode postMode)
    {
        var effective = postMode == IdentityMode.Default
            ? Map(author.DisplayNamePreference)
            : postMode;

        if (effective == IdentityMode.Anonymous)
            return new AuthorInfo { DisplayName = AnonymousName, EnrollmentNumber = null, IsAnonymous = true };

        return new AuthorInfo
        {
            DisplayName = effective == IdentityMode.RealName ? author.FullName : author.Username,
            EnrollmentNumber = author.ShowEnrollmentNumber ? author.EnrollmentNumber : null,
            IsAnonymous = false,
        };
    }

    private static IdentityMode Map(DisplayNamePreference pref) => pref switch
    {
        DisplayNamePreference.RealName => IdentityMode.RealName,
        DisplayNamePreference.Anonymous => IdentityMode.Anonymous,
        _ => IdentityMode.Username,
    };
}
