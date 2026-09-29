namespace HexArch.Services.IdentityAccess.Ports.Input.Commands.UpdateProfile
{
    public interface IUpdateProfileCommandHandler
    {
        Task<UpdateProfileResult> Handle(UpdateProfileCommand command, CancellationToken cancellationToken);
    }
}
