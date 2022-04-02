using Core.Enums;
using System.Collections.Generic;

namespace Core.Models.WorkItem
{
    public class InterruptionItemBase
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Priority Priority { get; set; }
        public List<InterruptionChecklistEntry> Checklists { get; set; } = new List<InterruptionChecklistEntry>();
    }
}
