namespace HexArch.Services.IdentityAccess.Ports.Input.Commands.RevokeRole
{
    public record RevokeRoleCommand(Guid UserId, string RoleName);
}
