using HexArch.Services.IdentityAccess.Ports.Output.Queries.ReadModels;

namespace HexArch.Services.IdentityAccess.Ports.Output.Queries
{
    public interface IUserSearchQuery
    {
        Task<UserSearchPage> SearchUsers(UserSearchCriteria criteria, CancellationToken cancellationToken = default);
    }
}
