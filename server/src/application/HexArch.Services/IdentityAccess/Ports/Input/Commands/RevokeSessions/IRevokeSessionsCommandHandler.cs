namespace HexArch.Services.IdentityAccess.Ports.Input.Commands.RevokeSessions
{
    public interface IRevokeSessionsCommandHandler
    {
        Task<RevokeSessionsResult> Handle(RevokeSessionsCommand command, CancellationToken cancellationToken);
    }
}
