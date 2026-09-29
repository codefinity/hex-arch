namespace HexArch.Services.IdentityAccess.Ports.Input.Commands.RequestEmailVerification
{
    public class RequestEmailVerificationResult
    {
        public bool Success { get; }
        public Guid UserId { get; }
        public IReadOnlyList<string> Errors { get; }

        private RequestEmailVerificationResult(bool success, Guid userId, IReadOnlyList<string> errors)
        {
            Success = success;
            UserId = userId;
            Errors = errors;
        }

        public static RequestEmailVerificationResult Succeeded(Guid userId) =>
            new(true, userId, Array.Empty<string>());

        public static RequestEmailVerificationResult Failed(params string[] errors) =>
            new(false, Guid.Empty, errors);
    }
}
