using Core.Models.WorkItem;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Core.DbContexts.Configurations
{
    public class InterruptionItemEntityTypeConfiguration : IEntityTypeConfiguration<InterruptionItem>
    {
        public void Configure(EntityTypeBuilder<InterruptionItem> builder)
        {
            builder.HasKey(_ => _.Id);
            builder.Property(_ => _.Name).HasMaxLength(140).IsRequired();
            builder.Property(_ => _.CreationTime).HasColumnType("timestamp with time zone");
            builder.Property(_ => _.ModifiedTime).HasColumnType("timestamp with time zone");
            builder.Property(_ => _.ResolvedTime).HasColumnType("timestamp with time zone");
        }
    }
}
