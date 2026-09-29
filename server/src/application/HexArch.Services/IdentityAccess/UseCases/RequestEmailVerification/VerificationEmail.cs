using HexArch.Services.IdentityAccess.Ports.Output.Email;

namespace HexArch.Services.IdentityAccess.UseCases.RequestEmailVerification
{
    /// <summary>
    /// The verification email, shared by RequestEmailVerification and ChangeEmail so both issue the
    /// same kind of token with the same lifetime.
    /// </summary>
    internal static class VerificationEmail
    {
        public static readonly TimeSpan TokenLifetime = TimeSpan.FromHours(24);

        public static EmailMessage Compose(string to, string name, string token, DateTime expiresOnUtc) =>
            new(to,
                "Verify your HexArch email address",
                $"Hi {name},\n\nUse this code to verify your email address:\n\n{token}\n\n" +
                $"It expires at {expiresOnUtc:u}.\n\nThanks,\nThe HexArch Team");
    }
}
