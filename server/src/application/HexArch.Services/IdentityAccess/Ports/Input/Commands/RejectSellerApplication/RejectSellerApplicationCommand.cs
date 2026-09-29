namespace HexArch.Services.IdentityAccess.Ports.Input.Commands.RejectSellerApplication
{
    public record RejectSellerApplicationCommand(Guid ApplicationId, string Reason);
}
