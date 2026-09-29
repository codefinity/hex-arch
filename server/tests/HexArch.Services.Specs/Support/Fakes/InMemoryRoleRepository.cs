using HexArch.Models.IdentityAccess;
using HexArch.Services.IdentityAccess.Ports.Output.Persistance;
using Moq;

namespace HexArch.Services.Specs.Support.Fakes
{
    public class InMemoryRoleRepository
    {
        private readonly Dictionary<string, Role> roles = new(StringComparer.OrdinalIgnoreCase);
        private readonly Mock<IRoleRepository> mock = new();

        public InMemoryRoleRepository()
        {
            mock.Setup(repository => repository.GetRoleByName(It.IsAny<string>()))
                .Returns<string>(name => Task.FromResult(roles.TryGetValue(name, out var role) ? role : null));
        }

        public IRoleRepository Object => mock.Object;

        public void Add(string name) => roles[name] = new Role { Id = Guid.NewGuid(), Name = name };

        public void Remove(string name) => roles.Remove(name);
    }
}
