using Core.Enums;
using Core.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Models.WorkItem
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
        public DateTime CreationTime { get => _creationTime.ToKindUtc(); set => _creationTime = value; }
        [Column(TypeName = "timestamp without time zone")]
        public DateTime ModifiedTime { get => _modifiedTime.ToKindUtc(); set => _modifiedTime = value; }
        [Column(TypeName = "timestamp without time zone")]
        public DateTime? ResolvedTime { get => _resolvedTime.ToKindUtc(); set => _resolvedTime = value; }
        public bool IsDeleted { get; set; }
        // relationships and navigations
        public List<InterruptionChecklistEntry> Checklists { get; set; } = new List<InterruptionChecklistEntry>();
        private DateTime _creationTime;
        private DateTime _modifiedTime;
        private DateTime? _resolvedTime;
    }
}
