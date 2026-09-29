namespace HexArch.Services.IdentityAccess.Ports.Input.Commands.SignIn
{
    public interface ISignInCommandHandler
    {
        Task<SignInResult> Handle(SignInCommand command, CancellationToken cancellationToken);
    }
}
