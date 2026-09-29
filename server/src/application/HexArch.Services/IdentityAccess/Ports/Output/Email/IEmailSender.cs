namespace HexArch.Services.IdentityAccess.Ports.Output.Email
{
    public interface IEmailSender
    {
        Task Send(EmailMessage message, CancellationToken cancellationToken = default);
    }
}
