using HexArch.Models.IdentityAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace HexArch.Persistance.Postgres.Configurations
{
    public class UserRoleEntityTypeConfiguration : IEntityTypeConfiguration<UserRole>
    {
        public void Configure(EntityTypeBuilder<UserRole> builder)
        {
            builder.ToTable("userroles", "users");

            builder.Property<Guid>("UserId").HasColumnName("userid");
            builder.Property<Guid>("RoleId").HasColumnName("roleid");

        }
    }
}
