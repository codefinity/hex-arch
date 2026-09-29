using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HexArch.Models.IdentityAccess;
using HexArch.Services.IdentityAccess.Ports.Output.Authentication;
using Microsoft.IdentityModel.Tokens;

namespace HexArch.Authentication.Jwt
{
    public class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly JwtOptions options;

        public JwtTokenGenerator(JwtOptions options)
        {
            this.options = options;
        }

        public AuthToken GenerateToken(User user)
        {
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Email, user.Email),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(ClaimTypes.Name, user.Name),
                new(HexArchClaimTypes.SecurityStamp, user.SecurityStamp.ToString())
            };

            claims.AddRange(user.Roles.Select(role => new Claim(ClaimTypes.Role, role.Name)));

            var signingCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.SigningKey)),
                SecurityAlgorithms.HmacSha256);

            var expiresOnUtc = DateTime.UtcNow.AddMinutes(options.ExpiryMinutes);

            var token = new JwtSecurityToken(
                issuer: options.Issuer,
                audience: options.Audience,
                claims: claims,
                expires: expiresOnUtc,
                signingCredentials: signingCredentials);

            var tokenValue = new JwtSecurityTokenHandler().WriteToken(token);

            return new AuthToken(tokenValue, expiresOnUtc);
        }
    }
}
