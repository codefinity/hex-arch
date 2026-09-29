namespace HexArch.Services.IdentityAccess.Ports.Input.Commands.ChangeEmail
{
    public record ChangeEmailCommand(string NewEmail, string CurrentPassword);
}
