using HexArch.Models.IdentityAccess;
using HexArch.Services.IdentityAccess.Ports.Output.Persistance;
using Moq;

namespace HexArch.Services.Specs.Support.Fakes
{
    // A Mock<IProfileRepository> backed by an in-memory list, so scenario steps can assert
    // on stored state (Profiles, Seed) while the handler talks to the mocked port.
    public class InMemoryProfileRepository
    {
        private readonly List<Profile> profiles = new();
        private readonly Mock<IProfileRepository> mock = new();

        public InMemoryProfileRepository()
        {
            mock.Setup(repository => repository.GetProfile(It.IsAny<Guid>()))
                .Returns<Guid>(userId => Task.FromResult(profiles.FirstOrDefault(p => p.UserId == userId)));

            mock.Setup(repository => repository.AddProfile(It.IsAny<Profile>()))
                .Callback<Profile>(profile => profiles.Add(profile))
                .Returns(Task.CompletedTask);

            mock.Setup(repository => repository.UpdateProfile(It.IsAny<Profile>()))
                .Callback<Profile>(profile =>
                {
                    var index = profiles.FindIndex(p => p.UserId == profile.UserId);
                    if (index >= 0)
                        profiles[index] = profile;
                })
                .Returns(Task.CompletedTask);

            mock.Setup(repository => repository.DeleteProfile(It.IsAny<Guid>()))
                .Callback<Guid>(userId => profiles.RemoveAll(p => p.UserId == userId))
                .Returns(Task.CompletedTask);
        }

        public IProfileRepository Object => mock.Object;
        public IReadOnlyList<Profile> Profiles => profiles;

        public void Seed(Profile profile) => profiles.Add(profile);

        public Task<Profile?> GetProfile(Guid userId) => Object.GetProfile(userId);
    }
}
