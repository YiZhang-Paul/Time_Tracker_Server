using Core.Models.WorkItem;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Core.DbContexts.Configurations
{
    public class TaskChecklistEntryEntityTypeConfiguration : IEntityTypeConfiguration<TaskChecklistEntry>
    {
        public void Configure(EntityTypeBuilder<TaskChecklistEntry> builder)
        {
            builder.HasKey(_ => _.Id);
            builder.Property(_ => _.Description).HasMaxLength(100).IsRequired();
            builder.Property(_ => _.Rank).HasMaxLength(15).IsRequired();
        }
    }
}
