using Core.Models.Event;
using Core.Models.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Core.DbContexts.Configurations
{
    public class EventPromptEntityTypeConfiguration : IEntityTypeConfiguration<EventPrompt>
    {
        public void Configure(EntityTypeBuilder<EventPrompt> builder)
        {
            builder.HasKey(_ => _.Id);
            builder.HasOne<UserProfile>().WithMany().HasForeignKey(_ => _.UserId);
            builder.Property(_ => _.PromptType).IsRequired();
            builder.Property(_ => _.ConfirmType).IsRequired();
            builder.Property(_ => _.Timestamp).HasColumnType("timestamp with time zone");
        }
    }
}
