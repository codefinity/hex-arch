namespace HexArch.Services.IdentityAccess.Ports.Input.Commands.ReactivateUser
{
    public record ReactivateUserCommand(Guid UserId, string Reason, string? OnlyIfDeactivatedFor = null);
}
