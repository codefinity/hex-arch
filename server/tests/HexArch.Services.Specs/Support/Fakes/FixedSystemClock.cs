using HexArch.Services.IdentityAccess.Ports.Output.Clock;
using Moq;

namespace HexArch.Services.Specs.Support.Fakes
{
    public class FixedSystemClock
    {
        private readonly Mock<ISystemClock> mock = new();

        public FixedSystemClock()
        {
            mock.Setup(clock => clock.UtcNow).Returns(() => UtcNow);
        }

        public DateTime UtcNow { get; set; } = DateTime.UtcNow;

        public ISystemClock Object => mock.Object;
    }
}
