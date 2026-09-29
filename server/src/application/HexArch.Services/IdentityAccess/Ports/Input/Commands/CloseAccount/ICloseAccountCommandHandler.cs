namespace HexArch.Services.IdentityAccess.Ports.Input.Commands.CloseAccount
{
    public interface ICloseAccountCommandHandler
    {
        Task<CloseAccountResult> Handle(CloseAccountCommand command, CancellationToken cancellationToken);
    }
}
