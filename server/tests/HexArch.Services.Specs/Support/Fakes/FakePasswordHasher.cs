using HexArch.Services.IdentityAccess.Ports.Output.Authentication;
using Moq;

namespace HexArch.Services.Specs.Support.Fakes
{
    // Deterministic and cheap: exercises the "never stored in plain text" and "is salted"
    // behaviours without paying for 100k rounds of PBKDF2 on every scenario.
    public class FakePasswordHasher
    {
        private const string FixedSalt = "fixed-test-salt";
        private readonly Mock<IPasswordHasher> mock = new();

        public FakePasswordHasher()
        {
            mock.Setup(hasher => hasher.Hash(It.IsAny<string>()))
                .Returns<string>(password => new PasswordHash($"hashed:{password}:{FixedSalt}", FixedSalt));

            mock.Setup(hasher => hasher.Verify(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns<string, string, string>((password, hash, salt) => hash == $"hashed:{password}:{salt}");
        }

        public IPasswordHasher Object => mock.Object;
    }
}
