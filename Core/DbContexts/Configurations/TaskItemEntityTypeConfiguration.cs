using Core.Models.WorkItem;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Core.DbContexts.Configurations
{
    public class TaskItemEntityTypeConfiguration : IEntityTypeConfiguration<TaskItem>
    {
        public void Configure(EntityTypeBuilder<TaskItem> builder)
        {
            builder.HasKey(_ => _.Id);
            builder.HasMany(_ => _.Checklists).WithOne().IsRequired().OnDelete(DeleteBehavior.Cascade);
            builder.Property(_ => _.Name).HasMaxLength(140).IsRequired();
            builder.Property(_ => _.CreationTime).HasColumnType("timestamp with time zone");
            builder.Property(_ => _.ModifiedTime).HasColumnType("timestamp with time zone");
            builder.Property(_ => _.ResolvedTime).HasColumnType("timestamp with time zone");
        }
    }
}
