namespace HexArch.Services.IdentityAccess.Ports.Output.Clock
{
    public interface ISystemClock
    {
        DateTime UtcNow { get; }
    }
}
