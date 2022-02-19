using Core.Extensions;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Models.WorkItem
{
    public partial class TaskItem : TaskItemBase
    {
        [Key]
        public long Id { get; set; }
        [Column(TypeName = "timestamp with time zone")]
        public DateTime CreationTime { get => _creationTime.ToKindUtc(); set => _creationTime = value; }
        [Column(TypeName = "timestamp with time zone")]
        public DateTime ModifiedTime { get => _modifiedTime.ToKindUtc(); set => _modifiedTime = value; }
        [Column(TypeName = "timestamp with time zone")]
        public DateTime? ResolvedTime { get => _resolvedTime.ToKindUtc(); set => _resolvedTime = value; }
        public bool IsDeleted { get; set; }
        private DateTime _creationTime;
        private DateTime _modifiedTime;
        private DateTime? _resolvedTime;
    }
}
