using HexArch.Services.IdentityAccess.Ports.Output.Authentication;
using Moq;

namespace HexArch.Services.Specs.Support.Fakes
{
    // Deterministic tokens ("token-1", "token-2", ...) and a readable hash, so steps can both
    // predict the code a user was emailed and seed a user with a code of their choosing.
    public class FakeSecureTokenGenerator
    {
        private readonly Mock<ISecureTokenGenerator> mock = new();
        private int issued;

        public FakeSecureTokenGenerator()
        {
            mock.Setup(generator => generator.Generate())
                .Returns(() =>
                {
                    LastIssued = $"token-{++issued}";
                    return new SecureToken(LastIssued, HashOf(LastIssued));
                });

            mock.Setup(generator => generator.Hash(It.IsAny<string>()))
                .Returns<string>(HashOf);
        }

        public string? LastIssued { get; private set; }

        public ISecureTokenGenerator Object => mock.Object;

        public static string HashOf(string token) => $"hashed:{token}";
    }
}
