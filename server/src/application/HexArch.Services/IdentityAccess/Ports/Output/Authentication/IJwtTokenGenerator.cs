using HexArch.Models.IdentityAccess;

namespace HexArch.Services.IdentityAccess.Ports.Output.Authentication
{
    public interface IJwtTokenGenerator
    {
        AuthToken GenerateToken(User user);
    }
}
