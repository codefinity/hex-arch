using HexArch.Services.IdentityAccess.Ports.Output.Email;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace HexArch.EMail
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly SmtpOptions options;

        public SmtpEmailSender(SmtpOptions options)
        {
            this.options = options;
        }

        public async Task Send(EmailMessage message, CancellationToken cancellationToken = default)
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
