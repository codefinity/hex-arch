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
        public DbSet<SellerApplication> SellerApplications { get; set; }
        public IdentityAccessContext(DbContextOptions<IdentityAccessContext> options) : base(options)
        {

        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            // Npgsql maps DateTime to timestamptz by default, which does not match the schema: every
            // column is `timestamp`. Without this, any entity read back and saved again fails to write.
            configurationBuilder.Properties<DateTime>()
                                .HaveColumnType("timestamp without time zone")
                                .HaveConversion<UtcDateTimeConverter>();
            configurationBuilder.Properties<DateTime?>()
                                .HaveColumnType("timestamp without time zone")
                                .HaveConversion<UtcDateTimeConverter>();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new UserEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new RoleEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new ProfileEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new SellerApplicationEntityTypeConfiguration());
            //modelBuilder.ApplyConfiguration(new UserRoleEntityTypeConfiguration());
        }
    }
}
