using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace LogiFlow.WebApi.Auth;

public sealed class JwtTokenService(IConfiguration configuration)
{
    public LoginResponse CreateToken(DemoUserOptions user)
    {
        var jwtSection = configuration.GetSection("Jwt");

        var issuer = jwtSection["Issuer"]
                     ?? throw new InvalidOperationException("Jwt:Issuer is missing.");

        var audience = jwtSection["Audience"]
                       ?? throw new InvalidOperationException("Jwt:Audience is missing.");

        var signingKey = jwtSection["SigningKey"]
                         ?? throw new InvalidOperationException("Jwt:SigningKey is missing.");

        var expiresAt = DateTimeOffset.UtcNow.AddHours(2);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Email),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        return new LoginResponse
        {
            AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role,
            ExpiresAt = expiresAt
        };
    }
}