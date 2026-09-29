namespace HexArch.Services.IdentityAccess.Ports.Input.Commands.SignOut
{
    public interface ISignOutCommandHandler
    {
        Task<SignOutResult> Handle(SignOutCommand command, CancellationToken cancellationToken);
    }
}
