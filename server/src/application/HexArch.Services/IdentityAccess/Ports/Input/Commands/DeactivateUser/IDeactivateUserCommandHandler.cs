namespace HexArch.Services.IdentityAccess.Ports.Input.Commands.DeactivateUser
{
    public interface IDeactivateUserCommandHandler
    {
        Task<DeactivateUserResult> Handle(DeactivateUserCommand command, CancellationToken cancellationToken);
    }
}
