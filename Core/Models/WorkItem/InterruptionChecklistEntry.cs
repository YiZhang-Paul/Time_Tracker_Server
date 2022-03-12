using System.ComponentModel.DataAnnotations;

namespace Core.Models.WorkItem
{
    public class InterruptionChecklistEntry
    {
        [Key]
        public long Id { get; set; }
        [Required]
        [StringLength(100)]
        public string Description { get; set; }
        [Required]
        [StringLength(15)]
        public string Rank { get; set; }
        public bool IsCompleted { get; set; }
    }
}
