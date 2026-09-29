namespace HexArch.Services.IdentityAccess.Ports.Output.Authentication
{
    public interface IPasswordHasher
    {
        PasswordHash Hash(string password);
        bool Verify(string password, string hash, string salt);
    }
}
