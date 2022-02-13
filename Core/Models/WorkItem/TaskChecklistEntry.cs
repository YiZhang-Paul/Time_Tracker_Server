using System.ComponentModel.DataAnnotations;

namespace Core.Models.WorkItem
{
    public class TaskChecklistEntry
    {
        [Key]
        public long Id { get; set; }
        [Required]
        [StringLength(100)]
        public string Description { get; set; }
        public bool IsCompleted { get; set; }
        // relationships and navigations
        public long ParentId { get; set; }
        public TaskItem Parent { get; set; }
    }
}
