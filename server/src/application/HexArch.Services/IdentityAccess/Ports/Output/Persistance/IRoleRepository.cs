using HexArch.Models.IdentityAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HexArch.Services.IdentityAccess.Ports.Output.Persistance
{
    public interface IRoleRepository
    {
        public Task<Role?> GetRoleByName(string roleName);

    }
}
