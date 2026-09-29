namespace HexArch.Services.IdentityAccess.Ports.Input.Commands.ApplyForSellerAccount
{
    public class ApplyForSellerAccountResult
    {
        public bool Success { get; }
        public Guid ApplicationId { get; }
        public IReadOnlyList<string> Errors { get; }

        private ApplyForSellerAccountResult(bool success, Guid applicationId, IReadOnlyList<string> errors)
        {
            Success = success;
            ApplicationId = applicationId;
            Errors = errors;
        }

        public static ApplyForSellerAccountResult Succeeded(Guid applicationId) =>
            new(true, applicationId, Array.Empty<string>());

        public static ApplyForSellerAccountResult Failed(params string[] errors) =>
            new(false, Guid.Empty, errors);
    }
}
