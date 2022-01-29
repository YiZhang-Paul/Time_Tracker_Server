using Core.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Models.Interruption
{
    public partial class InterruptionItem
    {
        [Key]
        public long Id { get; set; }
        [Required]
        [StringLength(140)]
        public string Name { get; set; }
        public string Description { get; set; }
        public Priority Priority { get; set; }
        [Column(TypeName = "timestamp without time zone")]
        public DateTime CreationTime { get; set; }
        [Column(TypeName = "timestamp without time zone")]
        public DateTime ModifiedTime { get; set; }
        [Column(TypeName = "timestamp without time zone")]
        public DateTime? ResolvedTime { get; set; }
        public bool IsDeleted { get; set; }
    }
}
