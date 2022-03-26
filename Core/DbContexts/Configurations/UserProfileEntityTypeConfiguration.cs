using Core.Models.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Core.DbContexts.Configurations
{
    public class UserProfileEntityTypeConfiguration : IEntityTypeConfiguration<UserProfile>
    {
        public void Configure(EntityTypeBuilder<UserProfile> builder)
        {
            builder.HasKey(_ => _.Id);
            builder.HasAlternateKey(_ => _.Email);
            builder.Property(_ => _.Email).HasMaxLength(320);
            builder.Property(_ => _.DisplayName).HasMaxLength(100).IsRequired();
            builder.Property(_ => _.CreationTime).HasColumnType("timestamp with time zone");
        }
    }
}
