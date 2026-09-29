using HexArch.Services.IdentityAccess.Ports.Output.Queries;
using HexArch.Services.IdentityAccess.Ports.Output.Queries.ReadModels;
using Moq;

namespace HexArch.Services.Specs.Support.Fakes
{
    // The filtering itself is SQL (viewmodels.search_user_viewmodels), so this fake does not repeat
    // it: it records the criteria the handler asked for and answers with a canned page.
    public class InMemoryUserSearchQuery
    {
        private readonly Mock<IUserSearchQuery> mock = new();

        public InMemoryUserSearchQuery()
        {
            mock.Setup(query => query.SearchUsers(It.IsAny<UserSearchCriteria>(), It.IsAny<CancellationToken>()))
                .Returns<UserSearchCriteria, CancellationToken>((criteria, _) =>
                {
                    LastCriteria = criteria;
                    return Task.FromResult(Page);
                });
        }

        public UserSearchCriteria? LastCriteria { get; private set; }

        public UserSearchPage Page { get; set; } = new(Array.Empty<UserSummaryReadModel>(), 0);

        public IUserSearchQuery Object => mock.Object;
    }
}
