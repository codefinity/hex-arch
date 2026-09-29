using HexArch.Models.IdentityAccess;
using HexArch.Services.IdentityAccess.Ports.Output.Persistance;
using Microsoft.EntityFrameworkCore;

namespace HexArch.Persistance.Postgres.IdentityAccess
{
    public class ProfileRepository : IProfileRepository
    {
        private readonly IdentityAccessContext identityAccessContext;

        public ProfileRepository(IdentityAccessContext identityAccessContext)
        {
            this.identityAccessContext = identityAccessContext;
        }

        public async Task AddProfile(Profile profile)
        {
            await identityAccessContext.AddAsync(profile);

            await identityAccessContext.SaveChangesAsync();
        }

        public async Task UpdateProfile(Profile profile)
        {
            identityAccessContext.Update(profile);

            await identityAccessContext.SaveChangesAsync();
        }

        public async Task<Profile?> GetProfile(Guid userId)
        {
            return await identityAccessContext.Profiles.Where(x => x.UserId == userId)
                                                        .FirstOrDefaultAsync();
        }

        public async Task DeleteProfile(Guid userId)
        {
            // A set-based delete that bypasses the change tracker, so no tracked User.Profile
            // navigation can resurrect the row on a later SaveChanges.
            await identityAccessContext.Profiles.Where(x => x.UserId == userId)
                                                .ExecuteDeleteAsync();
        }
    }
}
