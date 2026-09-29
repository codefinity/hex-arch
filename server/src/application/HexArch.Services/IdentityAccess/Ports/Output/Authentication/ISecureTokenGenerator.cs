namespace HexArch.Services.IdentityAccess.Ports.Output.Authentication
{
    /// <summary>
    /// Issues single-use secrets (password reset, email verification). Only the hash is ever
    /// stored, so a leaked users table does not hand out working tokens.
    /// </summary>
    public interface ISecureTokenGenerator
    {
        SecureToken Generate();
        string Hash(string token);
    }
}
