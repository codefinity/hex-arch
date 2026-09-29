namespace HexArch.Services.IdentityAccess.Ports.Input.Commands.ApproveSellerApplication
{
    public interface IApproveSellerApplicationCommandHandler
    {
        Task<ApproveSellerApplicationResult> Handle(ApproveSellerApplicationCommand command, CancellationToken cancellationToken);
    }
}
