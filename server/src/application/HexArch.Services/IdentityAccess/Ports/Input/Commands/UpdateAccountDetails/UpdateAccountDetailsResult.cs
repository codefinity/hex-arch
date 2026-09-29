namespace HexArch.Services.IdentityAccess.Ports.Input.Commands.UpdateAccountDetails
{
    public class UpdateAccountDetailsResult
    {
        public bool Success { get; }
        public Guid UserId { get; }
        public IReadOnlyList<string> Errors { get; }

        private UpdateAccountDetailsResult(bool success, Guid userId, IReadOnlyList<string> errors)
        {
            Success = success;
            UserId = userId;
            Errors = errors;
        }

        public static UpdateAccountDetailsResult Succeeded(Guid userId) =>
            new(true, userId, Array.Empty<string>());

        public static UpdateAccountDetailsResult Failed(params string[] errors) =>
            new(false, Guid.Empty, errors);
    }
}
