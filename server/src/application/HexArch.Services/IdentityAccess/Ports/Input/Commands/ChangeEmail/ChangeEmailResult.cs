namespace HexArch.Services.IdentityAccess.Ports.Input.Commands.ChangeEmail
{
    public class ChangeEmailResult
    {
        public bool Success { get; }
        public Guid UserId { get; }
        public IReadOnlyList<string> Errors { get; }

        private ChangeEmailResult(bool success, Guid userId, IReadOnlyList<string> errors)
        {
            Success = success;
            UserId = userId;
            Errors = errors;
        }

        public static ChangeEmailResult Succeeded(Guid userId) =>
            new(true, userId, Array.Empty<string>());

        public static ChangeEmailResult Failed(params string[] errors) =>
            new(false, Guid.Empty, errors);
    }
}
