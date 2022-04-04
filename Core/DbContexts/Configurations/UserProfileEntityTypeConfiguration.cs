using Core.Models.User;
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
            builder.Property(_ => _.DisplayName).HasMaxLength(25).IsRequired();
            builder.Property(_ => _.CreationTime).HasColumnType("timestamp with time zone");
        }
    }
}
