using HexArch.Models.IdentityAccess;

namespace HexArch.Services.IdentityAccess.Ports.Output.Persistance
{
    public interface IProfileRepository
    {
        Task<Profile?> GetProfile(Guid userId);
        Task AddProfile(Profile profile);
        Task UpdateProfile(Profile profile);
        // Idempotent: deleting a profile that does not exist is a no-op.
        Task DeleteProfile(Guid userId);
    }
}
