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
            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
