namespace HexArch.Services.IdentityAccess.Ports.Input.Queries.SearchUsers
{
    public interface ISearchUsersQueryHandler
    {
        Task<SearchUsersResult> Handle(SearchUsersQuery query, CancellationToken cancellationToken);
    }
}
