namespace HexArch.Services.IdentityAccess.Ports.Input.Commands.UpdateProfile
{
    public class UpdateProfileResult
    {
        public bool Success { get; }
        public Guid UserId { get; }
        public IReadOnlyList<string> Errors { get; }

        private UpdateProfileResult(bool success, Guid userId, IReadOnlyList<string> errors)
        {
            Success = success;
            UserId = userId;
            Errors = errors;
        }

        public static UpdateProfileResult Succeeded(Guid userId) =>
            new(true, userId, Array.Empty<string>());

        public static UpdateProfileResult Failed(params string[] errors) =>
            new(false, Guid.Empty, errors);
    }
}
