namespace HexArch.Services.IdentityAccess.Ports.Output.Authentication
{
    public record AuthToken(string Value, DateTime ExpiresOnUtc);
}
