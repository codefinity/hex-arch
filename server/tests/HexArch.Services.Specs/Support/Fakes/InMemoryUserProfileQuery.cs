using HexArch.Services.IdentityAccess.Ports.Output.Queries;
using HexArch.Services.IdentityAccess.Ports.Output.Queries.ReadModels;
using Moq;

namespace HexArch.Services.Specs.Support.Fakes
{
    // A Mock<IUserProfileQuery> backed by an in-memory list, so scenario steps can seed
    // the projected row the handler reads, without touching Postgres.
    public class InMemoryUserProfileQuery
    {
        private readonly List<UserProfileReadModel> profiles = new();
        private readonly Mock<IUserProfileQuery> mock = new();

        public InMemoryUserProfileQuery()
        {
            mock.Setup(query => query.GetUserProfile(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns<Guid, CancellationToken>((userId, _) => Task.FromResult(
                    profiles.FirstOrDefault(p => p.UserId == userId)));
        }

        public IUserProfileQuery Object => mock.Object;

        public void Seed(UserProfileReadModel profile) => profiles.Add(profile);
    }
}
