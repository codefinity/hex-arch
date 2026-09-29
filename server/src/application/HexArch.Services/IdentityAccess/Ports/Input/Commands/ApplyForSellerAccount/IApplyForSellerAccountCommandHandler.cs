namespace HexArch.Services.IdentityAccess.Ports.Input.Commands.ApplyForSellerAccount
{
    public interface IApplyForSellerAccountCommandHandler
    {
        Task<ApplyForSellerAccountResult> Handle(ApplyForSellerAccountCommand command, CancellationToken cancellationToken);
    }
}
