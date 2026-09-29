namespace HexArch.Services.IdentityAccess.Ports.Input.Commands.DeactivateUser
{
    public record DeactivateUserCommand(Guid UserId, string Reason);
}
