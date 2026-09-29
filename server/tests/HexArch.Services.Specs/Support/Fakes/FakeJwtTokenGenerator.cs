using HexArch.Models.IdentityAccess;
using HexArch.Services.IdentityAccess.Ports.Output.Authentication;
using Moq;

namespace HexArch.Services.Specs.Support.Fakes
{
    // Deterministic token issuance: the token value encodes the user id so step
    // definitions can assert who a token was issued for without a real JWT library.
    public class FakeJwtTokenGenerator
    {
        private readonly Mock<IJwtTokenGenerator> mock = new();

        public FakeJwtTokenGenerator()
        {
            mock.Setup(generator => generator.GenerateToken(It.IsAny<User>()))
                .Returns<User>(user => new AuthToken($"token-for:{user.Id}", ExpiresOnUtc));
        }

        public DateTime ExpiresOnUtc { get; set; } = DateTime.UtcNow;

        public IJwtTokenGenerator Object => mock.Object;
    }
}
