namespace HexArch.Services.IdentityAccess.Ports.Input.Commands.RevokeSessions
{
    public class RevokeSessionsResult
    {
        public bool Success { get; }
        public Guid UserId { get; }
        public IReadOnlyList<string> Errors { get; }

        private RevokeSessionsResult(bool success, Guid userId, IReadOnlyList<string> errors)
        {
            Success = success;
            UserId = userId;
            Errors = errors;
        }

        public static RevokeSessionsResult Succeeded(Guid userId) =>
            new(true, userId, Array.Empty<string>());

        public static RevokeSessionsResult Failed(params string[] errors) =>
            new(false, Guid.Empty, errors);
    }
}
