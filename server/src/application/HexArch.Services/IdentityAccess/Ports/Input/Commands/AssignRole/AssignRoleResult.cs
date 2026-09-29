namespace HexArch.Services.IdentityAccess.Ports.Input.Commands.AssignRole
{
    public class AssignRoleResult
    {
        public bool Success { get; }
        public Guid UserId { get; }
        public IReadOnlyList<string> Errors { get; }

        private AssignRoleResult(bool success, Guid userId, IReadOnlyList<string> errors)
        {
            Success = success;
            UserId = userId;
            Errors = errors;
        }

        public static AssignRoleResult Succeeded(Guid userId) =>
            new(true, userId, Array.Empty<string>());

        public static AssignRoleResult Failed(params string[] errors) =>
            new(false, Guid.Empty, errors);
    }
}
