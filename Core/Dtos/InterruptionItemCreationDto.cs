using Core.Enums;
using Core.Models.WorkItem;
using System.Collections.Generic;

namespace Core.Dtos
{
    public class InterruptionItemCreationDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Priority Priority { get; set; }
        public List<InterruptionChecklistEntry> Checklists { get; set; } = new List<InterruptionChecklistEntry>();
    }
}
