using HexArch.Models.IdentityAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HexArch.Persistance.Postgres.Configurations
{
    public class ProfileEntityTypeConfiguration : IEntityTypeConfiguration<Profile>
    {
        public void Configure(EntityTypeBuilder<Profile> builder)
        {
            builder.ToTable("profiles", "users");

            builder.HasKey(x => x.UserId);

            builder.Property<Guid>("UserId").HasColumnName("userid");
            builder.Property<string>("Bio").HasColumnName("bio");
            builder.Property<string>("Address").HasColumnName("address");
            builder.Property<DateTime?>("DateOfBirth").HasColumnName("dateofbirth");
            builder.Property<string>("AvatarUrl").HasColumnName("avatarurl");
            builder.Property<DateTime>("UpdatedOn").HasColumnName("updatedon");

            builder.HasOne(x => x.User)
                   .WithOne(x => x.Profile)
                   .HasForeignKey<Profile>(x => x.UserId);
        }
    }
}
