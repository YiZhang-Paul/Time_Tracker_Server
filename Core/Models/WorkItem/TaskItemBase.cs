using System.Collections.Generic;

namespace Core.Models.WorkItem
{
    public class TaskItemBase
    {
        public long UserId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Effort { get; set; }
        public List<TaskChecklistEntry> Checklists { get; set; } = new List<TaskChecklistEntry>();
    }
}
