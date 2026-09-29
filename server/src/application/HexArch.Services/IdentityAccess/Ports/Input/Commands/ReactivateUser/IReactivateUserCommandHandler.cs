namespace HexArch.Services.IdentityAccess.Ports.Input.Commands.ReactivateUser
{
    public interface IReactivateUserCommandHandler
    {
        Task<ReactivateUserResult> Handle(ReactivateUserCommand command, CancellationToken cancellationToken);
    }
}
