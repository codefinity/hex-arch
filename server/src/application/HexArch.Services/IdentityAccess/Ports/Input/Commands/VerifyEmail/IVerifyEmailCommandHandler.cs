namespace HexArch.Services.IdentityAccess.Ports.Input.Commands.VerifyEmail
{
    public interface IVerifyEmailCommandHandler
    {
        Task<VerifyEmailResult> Handle(VerifyEmailCommand command, CancellationToken cancellationToken);
    }
}
