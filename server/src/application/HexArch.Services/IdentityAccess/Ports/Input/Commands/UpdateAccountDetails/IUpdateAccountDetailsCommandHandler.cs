namespace HexArch.Services.IdentityAccess.Ports.Input.Commands.UpdateAccountDetails
{
    public interface IUpdateAccountDetailsCommandHandler
    {
        Task<UpdateAccountDetailsResult> Handle(UpdateAccountDetailsCommand command, CancellationToken cancellationToken);
    }
}
