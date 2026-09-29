using HexArch.Services.IdentityAccess.Ports.Output.Authentication;
using System.Security.Cryptography;

namespace HexArch.Authentication.Jwt
{
    public class Pbkdf2PasswordHasher : IPasswordHasher
    {
        private const int SaltSizeInBytes = 16;
        private const int KeySizeInBytes = 32;
        private const int Iterations = 100_000;
        private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

        public PasswordHash Hash(string password)
        {
            var saltBytes = RandomNumberGenerator.GetBytes(SaltSizeInBytes);
            var hashBytes = Rfc2898DeriveBytes.Pbkdf2(password, saltBytes, Iterations, Algorithm, KeySizeInBytes);

            return new PasswordHash(Convert.ToBase64String(hashBytes), Convert.ToBase64String(saltBytes));
        }

        public bool Verify(string password, string hash, string salt)
        {
            var saltBytes = Convert.FromBase64String(salt);
            var expectedHashBytes = Convert.FromBase64String(hash);
            var actualHashBytes = Rfc2898DeriveBytes.Pbkdf2(password, saltBytes, Iterations, Algorithm, KeySizeInBytes);

            return CryptographicOperations.FixedTimeEquals(actualHashBytes, expectedHashBytes);
        }
    }
}
