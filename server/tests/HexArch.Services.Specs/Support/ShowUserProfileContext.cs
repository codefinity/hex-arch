using HexArch.Services.IdentityAccess.Ports.Input.Queries.ShowUserProfile;
using HexArch.Services.IdentityAccess.UseCases.ShowUserProfile;
using HexArch.Services.Specs.Support.Fakes;

namespace HexArch.Services.Specs.Support
{
    // Injected fresh into each scenario by Reqnroll's context-injection container.
    public class ShowUserProfileContext
    {
        public InMemoryUserProfileQuery UserProfiles { get; } = new();
        public FakeCurrentUserProvider CurrentUser { get; } = new();

        public ShowUserProfileResult? Result { get; set; }

        public IShowUserProfileQueryHandler Handler =>
            new ShowUserProfileQueryHandler(CurrentUser.Object, UserProfiles.Object);
    }
}
