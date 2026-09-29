namespace HexArch.Services.IdentityAccess.Ports.Output.Authentication
{
    public record PasswordHash(string Hash, string Salt);
}
