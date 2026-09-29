using HexArch.Services.IdentityAccess.Ports.Input.Commands.SignIn;
using HexArch.Services.Specs.Support.Fakes;
using HexArch.Services.IdentityAccess.UseCases.SignIn;

namespace HexArch.Services.Specs.Support
{
    // Injected fresh into each scenario by Reqnroll's context-injection container.
    public class SignInContext
    {
        public InMemoryUserRepository Users { get; } = new();
        public FakePasswordHasher Hasher { get; } = new();
        public FakeJwtTokenGenerator TokenGenerator { get; } = new();
        public FixedSystemClock Clock { get; } = new();
        public RecordingEventDispatcher Events { get; } = new();

        public SignInResult? Result { get; set; }

        public ISignInCommandHandler Handler => new SignInCommandHandler(
            new SignInCommandValidator(), Users.Object, Hasher.Object, TokenGenerator.Object, Clock.Object, Events.Object);
    }
}
