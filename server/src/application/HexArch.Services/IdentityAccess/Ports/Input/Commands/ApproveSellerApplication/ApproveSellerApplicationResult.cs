namespace HexArch.Services.IdentityAccess.Ports.Input.Commands.ApproveSellerApplication
{
    public class ApproveSellerApplicationResult
    {
        public bool Success { get; }
        public Guid ApplicationId { get; }
        public IReadOnlyList<string> Errors { get; }

        private ApproveSellerApplicationResult(bool success, Guid applicationId, IReadOnlyList<string> errors)
        {
            Success = success;
            ApplicationId = applicationId;
            Errors = errors;
        }

        public static ApproveSellerApplicationResult Succeeded(Guid applicationId) =>
            new(true, applicationId, Array.Empty<string>());

        public static ApproveSellerApplicationResult Failed(params string[] errors) =>
            new(false, Guid.Empty, errors);
    }
}
