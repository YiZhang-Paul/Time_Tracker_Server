using Core.Models.Authentication;
using Core.Models.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Core.DbContexts.Configurations
{
    public class UserRefreshTokenEntityTypeConfiguration : IEntityTypeConfiguration<UserRefreshToken>
    {
        public void Configure(EntityTypeBuilder<UserRefreshToken> builder)
        {
            builder.HasKey(_ => _.UserId);
            builder.HasOne<UserProfile>().WithOne().HasForeignKey<UserRefreshToken>(_ => _.UserId);
            builder.HasIndex(_ => _.RefreshToken).IsUnique();
            builder.Property(_ => _.UserId).ValueGeneratedNever();
            builder.Property(_ => _.Guid).IsRequired();
            builder.Property(_ => _.RefreshToken).IsRequired();
            builder.Property(_ => _.ExpireTime).HasColumnType("timestamp with time zone");
        }
    }
}
