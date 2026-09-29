namespace HexArch.Services.IdentityAccess.Ports.Input.Commands.ReactivateUser
{
    public class ReactivateUserResult
    {
        public bool Success { get; }
        public Guid UserId { get; }
        public IReadOnlyList<string> Errors { get; }

        private ReactivateUserResult(bool success, Guid userId, IReadOnlyList<string> errors)
        {
            Success = success;
            UserId = userId;
            Errors = errors;
        }

        public static ReactivateUserResult Succeeded(Guid userId) =>
            new(true, userId, Array.Empty<string>());

        public static ReactivateUserResult Failed(params string[] errors) =>
            new(false, Guid.Empty, errors);
    }
}
