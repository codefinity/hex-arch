namespace HexArch.Services.IdentityAccess.Ports.Input.Commands.SignOut
{
    public class SignOutResult
    {
        public bool Success { get; }
        public Guid UserId { get; }
        public IReadOnlyList<string> Errors { get; }

        private SignOutResult(bool success, Guid userId, IReadOnlyList<string> errors)
        {
            Success = success;
            UserId = userId;
            Errors = errors;
        }

        public static SignOutResult Succeeded(Guid userId) =>
            new(true, userId, Array.Empty<string>());

        public static SignOutResult Failed(params string[] errors) =>
            new(false, Guid.Empty, errors);
    }
}
