using HexArch.Services.IdentityAccess.Ports.Input.Queries.ShowUserProfile;
using HexArch.Services.IdentityAccess.Ports.Output.Authorization;
using HexArch.Services.IdentityAccess.Ports.Output.Queries;

namespace HexArch.Services.IdentityAccess.UseCases.ShowUserProfile
{
    public class ShowUserProfileQueryHandler : IShowUserProfileQueryHandler
    {
        private readonly ICurrentUserProvider currentUserProvider;
        private readonly IUserProfileQuery userProfileQuery;

        public ShowUserProfileQueryHandler(ICurrentUserProvider currentUserProvider, IUserProfileQuery userProfileQuery)
        {
            this.currentUserProvider = currentUserProvider;
            this.userProfileQuery = userProfileQuery;
        }

        public async Task<ShowUserProfileResult> Handle(ShowUserProfileQuery query, CancellationToken cancellationToken)
        {
            // Reads the projected row only. A user whose projection has not landed yet reads as
            // missing; startup reconciliation repairs those rows.
            var profile = await userProfileQuery.GetUserProfile(currentUserProvider.UserId, cancellationToken);

            return profile is null
                ? ShowUserProfileResult.Failed("User not found.")
                : ShowUserProfileResult.Succeeded(profile);
        }
    }
}
