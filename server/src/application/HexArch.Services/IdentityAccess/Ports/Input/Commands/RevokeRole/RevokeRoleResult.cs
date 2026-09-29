namespace HexArch.Services.IdentityAccess.Ports.Input.Commands.RevokeRole
{
    public class RevokeRoleResult
    {
        public bool Success { get; }
        public Guid UserId { get; }
        public IReadOnlyList<string> Errors { get; }

        private RevokeRoleResult(bool success, Guid userId, IReadOnlyList<string> errors)
        {
            Success = success;
            UserId = userId;
            Errors = errors;
        }

        public static RevokeRoleResult Succeeded(Guid userId) =>
            new(true, userId, Array.Empty<string>());

        public static RevokeRoleResult Failed(params string[] errors) =>
            new(false, Guid.Empty, errors);
    }
}
