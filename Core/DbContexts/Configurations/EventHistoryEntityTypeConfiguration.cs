using Core.Models.Event;
using Core.Models.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Core.DbContexts.Configurations
{
    public class EventHistoryEntityTypeConfiguration : IEntityTypeConfiguration<EventHistory>
    {
        public void Configure(EntityTypeBuilder<EventHistory> builder)
        {
            builder.HasKey(_ => _.Id);
            builder.HasOne<UserProfile>().WithMany().HasForeignKey(_ => _.UserId);
            builder.Property(_ => _.ResourceId).IsRequired();
            builder.Property(_ => _.EventType).IsRequired();
            builder.Property(_ => _.Timestamp).HasColumnType("timestamp with time zone");
        }
    }
}
