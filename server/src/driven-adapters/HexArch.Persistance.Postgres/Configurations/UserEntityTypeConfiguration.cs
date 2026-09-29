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
    public class UserEntityTypeConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("users", "users");

            builder.HasKey(x => x.Id);

            builder.Property<Guid>("Id").HasColumnName("id");
            builder.Property<string>("Name").HasColumnName("name");
            builder.Property<string>("Email").HasColumnName("email");
            builder.Property<string>("Password").HasColumnName("password");
            builder.Property<string>("Salt").HasColumnName("salt");
            builder.Property<string>("MobileNo").HasColumnName("mobileno");
            builder.Property<bool>("Active").HasColumnName("active");
            builder.Property<DateTime>("RegisteredOn").HasColumnName("registeredon");

            builder.HasMany(e => e.Roles)
                   .WithMany()
                   .UsingEntity<Dictionary<string, object>>(
                       "userroles",
                       right => right.HasOne<Role>().WithMany().HasForeignKey("RoleId"),
                       left => left.HasOne<User>().WithMany().HasForeignKey("UserId"),
                       join =>
                       {
                           join.ToTable("userroles", "users");
                           join.Property<Guid>("UserId").HasColumnName("userid");
                           join.Property<Guid>("RoleId").HasColumnName("rolesid");
                       });

        }
    }
}
