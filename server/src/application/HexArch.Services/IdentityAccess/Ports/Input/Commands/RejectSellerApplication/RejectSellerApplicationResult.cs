namespace HexArch.Services.IdentityAccess.Ports.Input.Commands.RejectSellerApplication
{
    public class RejectSellerApplicationResult
    {
        public bool Success { get; }
        public Guid ApplicationId { get; }
        public IReadOnlyList<string> Errors { get; }

        private RejectSellerApplicationResult(bool success, Guid applicationId, IReadOnlyList<string> errors)
        {
            Success = success;
            ApplicationId = applicationId;
            Errors = errors;
        }

        public static RejectSellerApplicationResult Succeeded(Guid applicationId) =>
            new(true, applicationId, Array.Empty<string>());

        public static RejectSellerApplicationResult Failed(params string[] errors) =>
            new(false, Guid.Empty, errors);
    }
}
