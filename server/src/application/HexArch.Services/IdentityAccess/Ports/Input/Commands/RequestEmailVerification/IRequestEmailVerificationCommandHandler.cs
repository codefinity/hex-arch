namespace HexArch.Services.IdentityAccess.Ports.Input.Commands.RequestEmailVerification
{
    public interface IRequestEmailVerificationCommandHandler
    {
        Task<RequestEmailVerificationResult> Handle(RequestEmailVerificationCommand command, CancellationToken cancellationToken);
    }
}
