using HexArch.Models.IdentityAccess;
using HexArch.Services.IdentityAccess.Ports.Output.Persistance;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HexArch.Persistance.Postgres.IdentityAccess
{
    public class RoleRepository: IRoleRepository
    {
        private readonly IdentityAccessContext identityAccessContext;

        public RoleRepository(IdentityAccessContext identityAccessContext)
        {
            this.identityAccessContext = identityAccessContext;
        }

        public async Task<Role?> GetRoleByName(string roleName)
        {

            return await identityAccessContext.Role.FirstOrDefaultAsync(x => x.Name == roleName);

        }

    }
}
