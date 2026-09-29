using HexArch.Models.IdentityAccess;
using HexArch.Services.IdentityAccess.Ports.Output.Persistance;
using Moq;

namespace HexArch.Services.Specs.Support.Fakes
{
    // A Mock<IUserRepository> backed by an in-memory list, so scenario steps can assert
    // on stored state (Users, Seed) while the handler talks to the mocked port.
    public class InMemoryUserRepository
    {
        private readonly List<User> users = new();
        private readonly Mock<IUserRepository> mock = new();

        public InMemoryUserRepository()
        {
            mock.Setup(repository => repository.AddUser(It.IsAny<User>()))
                .Callback<User>(user =>
                {
                    // Mirrors Postgres, where users.id defaults to gen_random_uuid() — the
                    // handler relies on AddUser assigning the id.
                    user.Id = Guid.NewGuid();
                    users.Add(user);
                })
                .Returns(Task.CompletedTask);

            mock.Setup(repository => repository.UpdateUser(It.IsAny<User>()))
                .Callback<User>(user =>
                {
                    var index = users.FindIndex(u => u.Id == user.Id);
                    if (index >= 0)
                        users[index] = user;
                    UpdateCount++;
                })
                .Returns(Task.CompletedTask);

            mock.Setup(repository => repository.GetUser(It.IsAny<string>()))
                .Returns<string>(email => Task.FromResult(
                    users.FirstOrDefault(u => string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase))));

            mock.Setup(repository => repository.GetUser(It.IsAny<Guid>()))
                .Returns<Guid>(id => Task.FromResult(users.FirstOrDefault(u => u.Id == id)));

            mock.Setup(repository => repository.CountActiveUsersInRole(It.IsAny<string>()))
                .Returns<string>(roleName => Task.FromResult(
                    users.Count(u => u.Active && u.Roles.Any(role => role.Name == roleName))));
        }

        public IUserRepository Object => mock.Object;
        public IReadOnlyList<User> Users => users;
        // How many times a handler wrote a user back, so scenarios can assert "nothing was saved".
        public int UpdateCount { get; private set; }

        public void Seed(User user) => users.Add(user);

        public Task<User?> GetUser(string email) => Object.GetUser(email);
        public Task<User?> GetUser(Guid id) => Object.GetUser(id);
    }
}
