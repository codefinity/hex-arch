using HexArch.Services.IdentityAccess.Ports.Input.Commands.UpdateProfile;
using HexArch.Services.Specs.Support.Fakes;
using HexArch.Services.IdentityAccess.UseCases.UpdateProfile;

namespace HexArch.Services.Specs.Support
{
    // Injected fresh into each scenario by Reqnroll's context-injection container.
    public class UpdateProfileContext
    {
        public InMemoryUserRepository Users { get; } = new();
        public InMemoryProfileRepository Profiles { get; } = new();
        public FakeCurrentUserProvider CurrentUser { get; } = new();
        public FixedSystemClock Clock { get; } = new();
        public RecordingEventDispatcher Events { get; } = new();

        public UpdateProfileResult? Result { get; set; }

        public IUpdateProfileCommandHandler Handler => new UpdateProfileCommandHandler(
            new UpdateProfileCommandValidator(), CurrentUser.Object, Users.Object, Profiles.Object, Clock.Object, Events.Object);
    }
}
