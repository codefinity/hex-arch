namespace HexArch.Services.IdentityAccess.Ports.Input.Commands.UpdateProfile
{
    public record UpdateProfileCommand(string? Bio, string? Address, DateTime? DateOfBirth, string? AvatarUrl);
}
