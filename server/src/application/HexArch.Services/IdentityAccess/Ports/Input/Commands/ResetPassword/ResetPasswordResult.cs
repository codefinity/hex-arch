namespace HexArch.Services.IdentityAccess.Ports.Input.Commands.ResetPassword
{
    public class ResetPasswordResult
    {
        public bool Success { get; }
        public Guid UserId { get; }
        public IReadOnlyList<string> Errors { get; }

        private ResetPasswordResult(bool success, Guid userId, IReadOnlyList<string> errors)
        {
            Success = success;
            UserId = userId;
            Errors = errors;
        }

        public static ResetPasswordResult Succeeded(Guid userId) =>
            new(true, userId, Array.Empty<string>());

        public static ResetPasswordResult Failed(params string[] errors) =>
            new(false, Guid.Empty, errors);
    }
}
