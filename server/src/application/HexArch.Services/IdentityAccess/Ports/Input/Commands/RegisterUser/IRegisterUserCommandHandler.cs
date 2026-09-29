namespace HexArch.Services.IdentityAccess.Ports.Input.Commands.RegisterUser
{
    public interface IRegisterUserCommandHandler
    {
        public Task<RegisterUserResult> Handle(RegisterUserCommand command, CancellationToken cancellationToken);

    }
}
