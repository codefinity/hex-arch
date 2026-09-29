namespace HexArch.Services.IdentityAccess.Ports.Input.Commands.RevokeRole
{
    public interface IRevokeRoleCommandHandler
    {
        Task<RevokeRoleResult> Handle(RevokeRoleCommand command, CancellationToken cancellationToken);
    }
}
