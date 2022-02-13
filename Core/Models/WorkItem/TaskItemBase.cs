using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Core.Models.WorkItem
{
    public class TaskItemBase
    {
        [Required]
        [StringLength(140)]
        public string Name { get; set; }
        public string Description { get; set; }
        public int Effort { get; set; }
        // relationships and navigations
        public List<TaskChecklistEntry> Checklists { get; set; } = new List<TaskChecklistEntry>();
    }
}
