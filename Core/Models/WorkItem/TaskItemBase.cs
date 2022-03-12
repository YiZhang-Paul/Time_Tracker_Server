using System.Collections.Generic;

namespace Core.Models.WorkItem
{
    public class TaskItemBase
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Effort { get; set; }
        // relationships and navigations
        public List<TaskChecklistEntry> Checklists { get; set; } = new List<TaskChecklistEntry>();
    }
}
