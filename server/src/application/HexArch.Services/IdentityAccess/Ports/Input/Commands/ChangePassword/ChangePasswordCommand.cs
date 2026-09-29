namespace HexArch.Services.IdentityAccess.Ports.Input.Commands.ChangePassword
{
    public record ChangePasswordCommand(string CurrentPassword, string NewPassword);
}
