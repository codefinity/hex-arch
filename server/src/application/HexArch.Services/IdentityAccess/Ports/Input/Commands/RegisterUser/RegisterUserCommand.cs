namespace HexArch.Services.IdentityAccess.Ports.Input.Commands.RegisterUser
{
    public record RegisterUserCommand(string Name, string Email, string Password, string MobileNo);
}
