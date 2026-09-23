using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ReportU.Configuration;

namespace ReportU.Services;

/// <summary>Emisión de tokens JWT (claims: sub=userId, email, username, jti).</summary>
public interface IJwtTokenService
{
    (string Token, DateTime ExpiresAt) CreateToken(Guid userId, string email, string username);
}

public class JwtTokenService(IOptions<JwtOptions> options) : IJwtTokenService
{
    private readonly JwtOptions _options = options.Value;

    public (string Token, DateTime ExpiresAt) CreateToken(Guid userId, string email, string username)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(_options.ExpiresMinutes);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity([
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim("username", username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            ]),
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            Expires = expiresAt,
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256),
        };
        var token = new JwtSecurityTokenHandler().CreateToken(descriptor);
        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}

/// <summary>Hash BCrypt de contraseñas (contrato sencillo recomendado para la práctica).</summary>
public interface IPasswordService
{
    string Hash(string password);
    bool Verify(string password, string hash);
}

public class PasswordService : IPasswordService
{
    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password.Trim());
    public bool Verify(string password, string hash) => BCrypt.Net.BCrypt.Verify(password.Trim(), hash);
}

/// <summary>Usuario autenticado actual a partir de los claims del JWT.</summary>
public interface ICurrentUserService
{
    Guid? UserId { get; }
    bool IsAuthenticated { get; }
}

public class CurrentUserService(IHttpContextAccessor accessor) : ICurrentUserService
{
    public Guid? UserId
    {
        get
        {
            var sub = accessor.HttpContext?.User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                ?? accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(sub, out var id) ? id : null;
        }
    }

    public bool IsAuthenticated => UserId.HasValue;
}
