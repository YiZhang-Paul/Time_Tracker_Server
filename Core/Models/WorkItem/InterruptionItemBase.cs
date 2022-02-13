using Core.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Core.Models.WorkItem
{
    public class InterruptionItemBase
    {
        [Required]
        [StringLength(140)]
        public string Name { get; set; }
        public string Description { get; set; }
        public Priority Priority { get; set; }
        // relationships and navigations
        public List<InterruptionChecklistEntry> Checklists { get; set; } = new List<InterruptionChecklistEntry>();
    }
}
