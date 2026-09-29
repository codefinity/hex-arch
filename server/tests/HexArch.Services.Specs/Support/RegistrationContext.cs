using HexArch.Services.IdentityAccess.Ports.Input.Commands.RegisterUser;
using HexArch.Services.Specs.Support.Fakes;
using HexArch.Services.IdentityAccess.UseCases.RegisterUser;

namespace HexArch.Services.Specs.Support
{
    // Injected fresh into each scenario by Reqnroll's context-injection container.
    public class RegistrationContext
    {
        public InMemoryUserRepository Users { get; } = new();
        public InMemoryRoleRepository Roles { get; } = new();
        public FakePasswordHasher Hasher { get; } = new();
        public FixedSystemClock Clock { get; } = new();
        public RecordingEventDispatcher Events { get; } = new();

        public RegisterUserResult? Result { get; set; }
        public int UserCountBeforeAttempt { get; set; }

        public IRegisterUserCommandHandler Handler => new RegisterUserCommandHandler(
            new RegisterUserCommandValidator(),
            Users.Object, Roles.Object, Hasher.Object, Clock.Object, Events.Object);
    }
}
