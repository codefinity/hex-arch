namespace HexArch.Services.IdentityAccess.Ports.Input.Commands.AssignRole
{
    public record AssignRoleCommand(Guid UserId, string RoleName);
}
