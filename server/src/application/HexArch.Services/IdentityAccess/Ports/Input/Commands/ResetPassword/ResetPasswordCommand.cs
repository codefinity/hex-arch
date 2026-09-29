namespace HexArch.Services.IdentityAccess.Ports.Input.Commands.ResetPassword
{
    public record ResetPasswordCommand(string Email, string Token, string NewPassword);
}
