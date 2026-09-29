using HexArch.Services.IdentityAccess.Ports.Output.Email;
using Moq;

namespace HexArch.Services.Specs.Support.Fakes
{
    public class RecordingEmailSender
    {
        private readonly List<EmailMessage> sent = new();
        private readonly Mock<IEmailSender> mock = new();

        public RecordingEmailSender()
        {
            mock.Setup(sender => sender.Send(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
                .Returns<EmailMessage, CancellationToken>((message, _) =>
                {
                    if (Unavailable)
                    {
                        throw new InvalidOperationException("The mail server is unavailable.");
                    }

                    sent.Add(message);
                    return Task.CompletedTask;
                });
        }

        // When set, every send throws, the way SmtpEmailSender does when the server is down.
        public bool Unavailable { get; set; }

        public IEmailSender Object => mock.Object;
        public IReadOnlyList<EmailMessage> Sent => sent;
    }
}
