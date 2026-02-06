using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LegalCaseManagement.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace LegalCaseManagement.Infrastructure.Auth
{
    public interface ITokenService
    {
        string CreateToken(User user, IEnumerable<string> roles);
    }

    public class TokenService : ITokenService
    {
        private readonly AuthOptions _options;

        public TokenService(IOptions<AuthOptions> options)
        {
            _options = options.Value;
        }

        public string CreateToken(User user, IEnumerable<string> roles)
        {
            if (string.IsNullOrWhiteSpace(_options.JwtKey))
            {
                throw new InvalidOperationException("Auth:JwtKey is not configured");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.JwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new("userId", user.UserId.ToString()),
                new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new(JwtRegisteredClaimNames.GivenName, user.FirstName ?? string.Empty),
                new(JwtRegisteredClaimNames.FamilyName, user.LastName ?? string.Empty),
                new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new(ClaimTypes.Name, user.FullName)
            };

            foreach (var role in roles.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var token = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
