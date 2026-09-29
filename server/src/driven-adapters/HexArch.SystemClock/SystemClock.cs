using HexArch.Services.IdentityAccess.Ports.Output.Clock;

namespace HexArch.SystemClock
{
    public class SystemClock : ISystemClock
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
