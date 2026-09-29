using HexArch.Models.IdentityAccess;

namespace HexArch.Services.IdentityAccess.Ports.Output.Persistance
{
    public interface ISellerApplicationRepository
    {
        Task AddApplication(SellerApplication application);
        Task UpdateApplication(SellerApplication application);
        Task<SellerApplication?> GetApplication(Guid applicationId);
        Task<SellerApplication?> GetPendingApplicationForUser(Guid userId);
    }
}
