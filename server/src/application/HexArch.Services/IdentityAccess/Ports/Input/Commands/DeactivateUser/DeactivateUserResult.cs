namespace HexArch.Services.IdentityAccess.Ports.Input.Commands.DeactivateUser
{
    public class DeactivateUserResult
    {
        public bool Success { get; }
        public Guid UserId { get; }
        public IReadOnlyList<string> Errors { get; }

        private DeactivateUserResult(bool success, Guid userId, IReadOnlyList<string> errors)
        {
            Success = success;
            UserId = userId;
            Errors = errors;
        }

        public static DeactivateUserResult Succeeded(Guid userId) =>
            new(true, userId, Array.Empty<string>());

        public static DeactivateUserResult Failed(params string[] errors) =>
            new(false, Guid.Empty, errors);
    }
}
