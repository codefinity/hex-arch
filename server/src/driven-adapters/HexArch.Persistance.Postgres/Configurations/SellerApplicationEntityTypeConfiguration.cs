using HexArch.Models.IdentityAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HexArch.Persistance.Postgres.Configurations
{
    public class SellerApplicationEntityTypeConfiguration : IEntityTypeConfiguration<SellerApplication>
    {
        public void Configure(EntityTypeBuilder<SellerApplication> builder)
        {
            builder.ToTable("sellerapplications", "users");

            builder.HasKey(x => x.Id);

            builder.Property<Guid>("Id").HasColumnName("id");
            builder.Property<Guid>("UserId").HasColumnName("userid");
            builder.Property<string>("BusinessName").HasColumnName("businessname");
            // Stored by name, so the column reads sensibly in SQL and in the projected view model.
            builder.Property(x => x.Status).HasColumnName("status").HasConversion<string>();
            builder.Property<DateTime>("SubmittedOn").HasColumnName("submittedon");
            builder.Property<DateTime?>("DecidedOn").HasColumnName("decidedon");
            builder.Property<Guid?>("DecidedBy").HasColumnName("decidedby");
            builder.Property<string>("DecisionNote").HasColumnName("decisionnote");
        }
    }
}
