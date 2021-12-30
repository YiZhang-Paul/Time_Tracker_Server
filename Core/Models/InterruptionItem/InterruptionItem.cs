using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace Core.Models.Task
{
    public partial class InterruptionItem
    {
        [Key]
        public long Id { get; set; }
        [Required]
        [StringLength(140)]
        public string Name { get; set; }
        public string Description { get; set; }
        public int Priority { get; set; }
        [Column(TypeName = "timestamp with time zone")]
        public DateTime CreationTime { get; set; }
        [Column(TypeName = "timestamp with time zone")]
        public DateTime ModifiedTime { get; set; }
    }
}
