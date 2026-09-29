using HexArch.Services.IdentityAccess.Ports.Output.Email;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace HexArch.EMail
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly SmtpOptions options;
        private readonly ILogger<SmtpEmailSender> logger;

        public SmtpEmailSender(SmtpOptions options, ILogger<SmtpEmailSender> logger)
        {
            this.options = options;
            this.logger = logger;
        }

        public async Task Send(EmailMessage message, CancellationToken cancellationToken = default)
        {
            try
            {
                await SendViaSmtp(message, cancellationToken);
            }
            catch (Exception exception)
            {
                // Logged here as well as rethrown: some callers (password reset) must swallow the failure
                // so it can't reveal whether an account exists, and this is then the only record of it.
                logger.LogError(exception, "Failed to send email \"{Subject}\".", message.Subject);
                throw;
            }
        }

        private async Task SendViaSmtp(EmailMessage message, CancellationToken cancellationToken)
        {
            var mimeMessage = new MimeMessage();
            mimeMessage.From.Add(MailboxAddress.Parse(options.From));
            mimeMessage.To.Add(MailboxAddress.Parse(message.To));
            mimeMessage.Subject = message.Subject;
            mimeMessage.Body = new TextPart("plain") { Text = message.Body };

            using var client = new SmtpClient();

            // Auto picks the right transport per target: implicit TLS on 465, STARTTLS where the
            // server advertises it (e.g. Gmail on 587), plain otherwise (e.g. Mailpit on 1025).
            await client.ConnectAsync(options.Host, options.Port, SecureSocketOptions.Auto, cancellationToken);

            if (!string.IsNullOrEmpty(options.Username))
            {
                if (string.IsNullOrEmpty(options.Password))
                {
                    throw new InvalidOperationException(
                        "Smtp:Username is configured but Smtp:Password is missing. " +
                        "Set it via `dotnet user-secrets set \"Smtp:Password\" \"<app-password>\" --project src/driving-adapters/HexArch.API/HexArch.API.csproj`.");
                }

                await client.AuthenticateAsync(options.Username, options.Password, cancellationToken);
            }

            await client.SendAsync(mimeMessage, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);
        }
    }
}
