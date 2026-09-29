namespace HexArch.Services.IdentityAccess.Ports.Input.Queries.ValidateSession
{
    public record ValidateSessionQuery(Guid UserId, Guid SecurityStamp);
}
