namespace HexArch.EMail
{
    public record SmtpOptions(string Host, int Port, string From, string? Username = null, string? Password = null);
}
