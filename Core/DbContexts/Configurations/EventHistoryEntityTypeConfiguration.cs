using Core.Models.Event;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Core.DbContexts.Configurations
{
    public class EventHistoryEntityTypeConfiguration : IEntityTypeConfiguration<EventHistory>
    {
        public void Configure(EntityTypeBuilder<EventHistory> builder)
        {
            builder.HasKey(_ => _.Id);
            builder.Property(_ => _.ResourceId).IsRequired();
            builder.Property(_ => _.EventType).IsRequired();
            builder.Property(_ => _.Timestamp).HasColumnType("timestamp with time zone");
        }
    }
}
