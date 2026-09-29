namespace HexArch.Services.IdentityAccess.Ports.Input.Commands.SignIn
{
    public class SignInResult
    {
        public bool Success { get; }
        public string? Token { get; }
        public DateTime? ExpiresOnUtc { get; }
        public IReadOnlyList<string> Errors { get; }

        private SignInResult(bool success, string? token, DateTime? expiresOnUtc, IReadOnlyList<string> errors)
        {
            Success = success;
            Token = token;
            ExpiresOnUtc = expiresOnUtc;
            Errors = errors;
        }

        public static SignInResult Succeeded(string token, DateTime expiresOnUtc) =>
            new(true, token, expiresOnUtc, Array.Empty<string>());

        public static SignInResult Failed(params string[] errors) =>
            new(false, null, null, errors);
    }
}
