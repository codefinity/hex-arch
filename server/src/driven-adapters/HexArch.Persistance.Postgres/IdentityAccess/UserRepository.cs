using HexArch.Models.IdentityAccess;
using HexArch.Services.IdentityAccess.Ports.Output.Persistance;
using Microsoft.EntityFrameworkCore;

namespace HexArch.Persistance.Postgres.IdentityAccess
{
    public class UserRepository : IUserRepository
    {
        private readonly IdentityAccessContext identityAccessContext;

        public UserRepository(IdentityAccessContext identityAccessContext)
        {
            this.identityAccessContext = identityAccessContext;
        }

        public async Task AddUser(User user)
        {
            await identityAccessContext.AddAsync(user);

            await identityAccessContext.SaveChangesAsync();
        }

        public async Task UpdateUser(User user)
        {
            identityAccessContext.Update(user);

            await identityAccessContext.SaveChangesAsync();
        }

        public async Task<User?> GetUser(string email)
        {
            return await identityAccessContext.Users.Where(x => x.Email == email)
                                                    .Include(r => r.Roles)
                                                    .FirstOrDefaultAsync();
        }

        public async Task<User?> GetUser(Guid id)
        {
            return await identityAccessContext.Users.Where(x => x.Id == id)
                                                    .Include(r => r.Roles)
                                                    .FirstOrDefaultAsync();
        }
    }
}
