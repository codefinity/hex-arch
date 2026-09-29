namespace HexArch.Services.IdentityAccess.Ports.Input.Commands.RejectSellerApplication
{
    public interface IRejectSellerApplicationCommandHandler
    {
        Task<RejectSellerApplicationResult> Handle(RejectSellerApplicationCommand command, CancellationToken cancellationToken);
    }
}
