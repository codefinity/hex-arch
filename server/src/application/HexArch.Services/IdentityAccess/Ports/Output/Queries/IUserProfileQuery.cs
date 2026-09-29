using HexArch.Services.IdentityAccess.Ports.Output.Queries.ReadModels;

namespace HexArch.Services.IdentityAccess.Ports.Output.Queries
{
    public interface IUserProfileQuery
    {
        Task<UserProfileReadModel?> GetUserProfile(Guid userId, CancellationToken cancellationToken = default);
    }
}
