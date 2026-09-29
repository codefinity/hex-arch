namespace HexArch.Services.IdentityAccess.Ports.Input.Commands.ResetPassword
{
    public interface IResetPasswordCommandHandler
    {
        Task<ResetPasswordResult> Handle(ResetPasswordCommand command, CancellationToken cancellationToken);
    }
}
