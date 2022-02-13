using Core.Models.WorkItem;
using System.Collections.Generic;

namespace Core.Dtos
{
    public class TaskItemCreationDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Effort { get; set; }
        public List<TaskChecklistEntry> Checklists { get; set; } = new List<TaskChecklistEntry>();
    }
}
