namespace HexArch.Services.IdentityAccess.Ports.Input.Commands.VerifyEmail
{
    public class VerifyEmailResult
    {
        public bool Success { get; }
        public Guid UserId { get; }
        public IReadOnlyList<string> Errors { get; }

        private VerifyEmailResult(bool success, Guid userId, IReadOnlyList<string> errors)
        {
            Success = success;
            UserId = userId;
            Errors = errors;
        }

        public static VerifyEmailResult Succeeded(Guid userId) =>
            new(true, userId, Array.Empty<string>());

        public static VerifyEmailResult Failed(params string[] errors) =>
            new(false, Guid.Empty, errors);
    }
}
