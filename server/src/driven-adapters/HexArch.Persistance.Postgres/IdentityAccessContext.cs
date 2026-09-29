using HexArch.Models.IdentityAccess;
using HexArch.Persistance.Postgres.Configurations;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HexArch.Persistance.Postgres
{
    public class IdentityAccessContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Role { get; set; }
        public DbSet<Profile> Profiles { get; set; }
        public IdentityAccessContext(DbContextOptions<IdentityAccessContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new UserEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new RoleEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new ProfileEntityTypeConfiguration());
            //modelBuilder.ApplyConfiguration(new UserRoleEntityTypeConfiguration());
        }
    }
}
