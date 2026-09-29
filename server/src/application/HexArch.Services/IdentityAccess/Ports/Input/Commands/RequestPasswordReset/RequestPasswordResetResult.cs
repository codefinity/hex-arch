namespace HexArch.Services.IdentityAccess.Ports.Input.Commands.RequestPasswordReset
{
    // Deliberately carries no user id: the result must look the same whether or not the email
    // belongs to an account.
    public class RequestPasswordResetResult
    {
        public bool Success { get; }
        public IReadOnlyList<string> Errors { get; }

        private RequestPasswordResetResult(bool success, IReadOnlyList<string> errors)
        {
            Success = success;
            Errors = errors;
        }

        public static RequestPasswordResetResult Succeeded() =>
            new(true, Array.Empty<string>());

        public static RequestPasswordResetResult Failed(params string[] errors) =>
            new(false, errors);
    }
}
