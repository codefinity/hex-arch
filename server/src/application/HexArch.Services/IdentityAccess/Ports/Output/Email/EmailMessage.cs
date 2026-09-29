namespace HexArch.Services.IdentityAccess.Ports.Output.Email
{
    public record EmailMessage(string To, string Subject, string Body);
}
