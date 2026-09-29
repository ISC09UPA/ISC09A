using EasyMemories.Models;

namespace EasyMemories.Services;

public interface ITokenService
{
    (string Token, DateTimeOffset ExpiresAt) CreateToken(AppUser user);
}
