using HexArch.Models.IdentityAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HexArch.Services.IdentityAccess.Ports.Output.Persistance
{
    public interface IUserRepository
    {
        public Task AddUser(User user);
        public Task UpdateUser(User user);
        public Task<User?> GetUser(string email);
        public Task<User?> GetUser(Guid id);

    }
}
