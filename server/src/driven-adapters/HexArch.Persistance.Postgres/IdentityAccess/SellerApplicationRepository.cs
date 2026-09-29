using HexArch.Models.IdentityAccess;
using HexArch.Services.IdentityAccess.Ports.Output.Persistance;
using Microsoft.EntityFrameworkCore;

namespace HexArch.Persistance.Postgres.IdentityAccess
{
    public class SellerApplicationRepository : ISellerApplicationRepository
    {
        private readonly IdentityAccessContext identityAccessContext;

        public SellerApplicationRepository(IdentityAccessContext identityAccessContext)
        {
            this.identityAccessContext = identityAccessContext;
        }

        public async Task AddApplication(SellerApplication application)
        {
            await identityAccessContext.AddAsync(application);

            await identityAccessContext.SaveChangesAsync();
        }

        public async Task UpdateApplication(SellerApplication application)
        {
            identityAccessContext.Update(application);

            await identityAccessContext.SaveChangesAsync();
        }

        public async Task<SellerApplication?> GetApplication(Guid applicationId)
        {
            return await identityAccessContext.SellerApplications.Where(x => x.Id == applicationId)
                                                                  .FirstOrDefaultAsync();
        }

        public async Task<SellerApplication?> GetPendingApplicationForUser(Guid userId)
        {
            return await identityAccessContext.SellerApplications.Where(x => x.UserId == userId
                                                                          && x.Status == SellerApplicationStatus.Pending)
                                                                  .FirstOrDefaultAsync();
        }
    }
}
