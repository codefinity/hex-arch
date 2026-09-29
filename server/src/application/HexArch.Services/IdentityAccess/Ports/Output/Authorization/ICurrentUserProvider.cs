namespace HexArch.Services.IdentityAccess.Ports.Output.Authorization
{
    public interface ICurrentUserProvider
    {
        Guid UserId { get; }
    }
}
