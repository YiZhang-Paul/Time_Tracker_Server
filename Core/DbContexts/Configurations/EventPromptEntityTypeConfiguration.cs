using Core.Models.Event;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Core.DbContexts.Configurations
{
    public class EventPromptEntityTypeConfiguration : IEntityTypeConfiguration<EventPrompt>
    {
        public void Configure(EntityTypeBuilder<EventPrompt> builder)
        {
            builder.HasKey(_ => _.Id);
            builder.Property(_ => _.PromptType).IsRequired();
            builder.Property(_ => _.ConfirmType).IsRequired();
            builder.Property(_ => _.Timestamp).HasColumnType("timestamp with time zone");
        }
    }
}