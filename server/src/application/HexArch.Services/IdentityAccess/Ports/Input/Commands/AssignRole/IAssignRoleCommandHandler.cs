namespace HexArch.Services.IdentityAccess.Ports.Input.Commands.AssignRole
{
    public interface IAssignRoleCommandHandler
    {
        Task<AssignRoleResult> Handle(AssignRoleCommand command, CancellationToken cancellationToken);
    }
}
