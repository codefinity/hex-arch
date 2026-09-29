namespace HexArch.Services.IdentityAccess.Ports.Input.Commands.RegisterUser
{
    public class RegisterUserResult
    {
        public bool Success { get; }
        public Guid UserId { get; }
        public IReadOnlyList<string> Errors { get; }

        private RegisterUserResult(bool success, Guid userId, IReadOnlyList<string> errors)
        {
            Success = success;
            UserId = userId;
            Errors = errors;
        }

        public static RegisterUserResult Succeeded(Guid userId) =>
            new(true, userId, Array.Empty<string>());

        public static RegisterUserResult Failed(params string[] errors) =>
            new(false, Guid.Empty, errors);
    }
}
