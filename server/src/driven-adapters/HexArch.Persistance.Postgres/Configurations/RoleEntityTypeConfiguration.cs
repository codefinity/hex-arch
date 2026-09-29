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
    public class RoleEntityTypeConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.ToTable("roles", "users");

            builder.HasKey(x => x.Id);
            builder.Property<Guid>("Id").HasColumnName("id");
            builder.Property<string>("Name").HasColumnName("name");

        }
    }
}
