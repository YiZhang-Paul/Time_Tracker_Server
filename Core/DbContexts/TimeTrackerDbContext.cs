using Core.DbContexts.Configurations;
using Core.Models.Event;
using Core.Models.WorkItem;
using Microsoft.EntityFrameworkCore;

namespace Core.DbContexts
{
    public partial class TimeTrackerDbContext : DbContext
    {
        public TimeTrackerDbContext() { }
        public TimeTrackerDbContext(DbContextOptions<TimeTrackerDbContext> options) : base(options) { }

        public virtual DbSet<InterruptionItem> InterruptionItem { get; set; }
        public virtual DbSet<TaskItem> TaskItem { get; set; }
        public virtual DbSet<EventHistory> EventHistory { get; set; }
        public virtual DbSet<EventHistorySummary> EventHistorySummary { get; set; }
        public virtual DbSet<EventPrompt> EventPrompt { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql("Name=TimeTrackerDbConnectionString");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseSerialColumns();
            new InterruptionItemEntityTypeConfiguration().Configure(modelBuilder.Entity<InterruptionItem>());
            new InterruptionChecklistEntryEntityTypeConfiguration().Configure(modelBuilder.Entity<InterruptionChecklistEntry>());
            new TaskItemEntityTypeConfiguration().Configure(modelBuilder.Entity<TaskItem>());
            new TaskChecklistEntryEntityTypeConfiguration().Configure(modelBuilder.Entity<TaskChecklistEntry>());
            new EventHistoryEntityTypeConfiguration().Configure(modelBuilder.Entity<EventHistory>());
            new EventHistorySummaryEntityTypeConfiguration().Configure(modelBuilder.Entity<EventHistorySummary>());
            new EventPromptEntityTypeConfiguration().Configure(modelBuilder.Entity<EventPrompt>());
            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
