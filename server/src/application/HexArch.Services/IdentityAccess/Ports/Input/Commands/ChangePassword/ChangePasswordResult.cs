namespace HexArch.Services.IdentityAccess.Ports.Input.Commands.ChangePassword
{
    public class ChangePasswordResult
    {
        public bool Success { get; }
        public Guid UserId { get; }
        // Changing the password revokes every existing session, including the caller's, so a
        // fresh token is issued to keep the caller signed in.
        public string? Token { get; }
        public DateTime? ExpiresOnUtc { get; }
        public IReadOnlyList<string> Errors { get; }

        private ChangePasswordResult(bool success, Guid userId, string? token, DateTime? expiresOnUtc, IReadOnlyList<string> errors)
        {
            Success = success;
            UserId = userId;
            Token = token;
            ExpiresOnUtc = expiresOnUtc;
            Errors = errors;
        }

        public static ChangePasswordResult Succeeded(Guid userId, string token, DateTime expiresOnUtc) =>
            new(true, userId, token, expiresOnUtc, Array.Empty<string>());

        public static ChangePasswordResult Failed(params string[] errors) =>
            new(false, Guid.Empty, null, null, errors);
    }
}
