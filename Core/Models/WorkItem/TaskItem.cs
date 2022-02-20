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
        public DateTime CreationTime { get; set; }
        [Column(TypeName = "timestamp with time zone")]
        public DateTime ModifiedTime { get; set; }
        [Column(TypeName = "timestamp with time zone")]
        public DateTime? ResolvedTime { get; set; }
        public bool IsDeleted { get; set; }
    }
}
