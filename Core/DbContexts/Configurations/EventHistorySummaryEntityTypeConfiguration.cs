using Core.Models.Event;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Core.DbContexts.Configurations
{
    public class EventHistorySummaryEntityTypeConfiguration : IEntityTypeConfiguration<EventHistorySummary>
    {
        public void Configure(EntityTypeBuilder<EventHistorySummary> builder)
        {
            builder.HasKey(_ => _.Id);
            builder.Property(_ => _.Timestamp).HasColumnType("timestamp with time zone");
            builder.ToView("EventHistorySummary");
        }
    }
}
