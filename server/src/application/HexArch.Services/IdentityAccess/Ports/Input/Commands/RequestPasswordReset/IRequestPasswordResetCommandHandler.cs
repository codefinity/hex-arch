namespace HexArch.Services.IdentityAccess.Ports.Input.Commands.RequestPasswordReset
{
    public interface IRequestPasswordResetCommandHandler
    {
        Task<RequestPasswordResetResult> Handle(RequestPasswordResetCommand command, CancellationToken cancellationToken);
    }
}
