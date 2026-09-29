namespace HexArch.Services.IdentityAccess.Ports.Input.Queries.ValidateSession
{
    public class ValidateSessionResult
    {
        public bool Success { get; }
        public IReadOnlyList<string> Errors { get; }

        private ValidateSessionResult(bool success, IReadOnlyList<string> errors)
        {
            Success = success;
            Errors = errors;
        }

        public static ValidateSessionResult Succeeded() =>
            new(true, Array.Empty<string>());

        public static ValidateSessionResult Failed(params string[] errors) =>
            new(false, errors);
    }
}
