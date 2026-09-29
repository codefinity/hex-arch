using System.Security.Cryptography;
using System.Text;
using HexArch.Services.IdentityAccess.Ports.Output.Authentication;
using Microsoft.IdentityModel.Tokens;

namespace HexArch.Authentication.Jwt
{
    public class Sha256SecureTokenGenerator : ISecureTokenGenerator
    {
        private const int TokenSizeInBytes = 32;

        public SecureToken Generate()
        {
            // URL-safe, so the value survives being pasted into a link or a query string.
            var value = Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(TokenSizeInBytes));

            return new SecureToken(value, Hash(value));
        }

        // A plain SHA-256 is enough here, unlike for passwords: the input is 256 bits of randomness,
        // so there is nothing for a slow, salted hash to protect against.
        public string Hash(string token) =>
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
    }
}
