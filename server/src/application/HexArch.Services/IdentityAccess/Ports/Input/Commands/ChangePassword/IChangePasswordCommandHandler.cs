namespace HexArch.Services.IdentityAccess.Ports.Input.Commands.ChangePassword
{
    public interface IChangePasswordCommandHandler
    {
        Task<ChangePasswordResult> Handle(ChangePasswordCommand command, CancellationToken cancellationToken);
    }
}
