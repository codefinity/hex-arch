using HexArch.Services.IdentityAccess.Ports.Output.Authorization;
using Moq;

namespace HexArch.Services.Specs.Support.Fakes
{
    public class FakeCurrentUserProvider
    {
        private readonly Mock<ICurrentUserProvider> mock = new();

        public FakeCurrentUserProvider()
        {
            mock.Setup(provider => provider.UserId).Returns(() => UserId);
        }

        public Guid UserId { get; set; }

        public ICurrentUserProvider Object => mock.Object;
    }
}
