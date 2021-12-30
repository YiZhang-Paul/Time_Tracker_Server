using Core.Models.Task;
using Microsoft.EntityFrameworkCore;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace Core.DbContexts
{
    public partial class TimeTrackerDbContext : DbContext
    {
        public TimeTrackerDbContext()
        {
        }

        public TimeTrackerDbContext(DbContextOptions<TimeTrackerDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<TaskItem> TaskItem { get; set; }
        public virtual DbSet<InterruptionItem> InterruptionItem { get; set; }

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
