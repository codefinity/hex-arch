namespace HexArch.Services.IdentityAccess.Ports.Input.Commands.ChangeEmail
{
    public interface IChangeEmailCommandHandler
    {
        Task<ChangeEmailResult> Handle(ChangeEmailCommand command, CancellationToken cancellationToken);
    }
}
