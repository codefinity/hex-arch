namespace HexArch.Authentication.Jwt
{
    public record JwtOptions(string SigningKey, string Issuer, string Audience, int ExpiryMinutes);
}
