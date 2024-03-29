using System;

namespace Core.Models.WorkItem
{
    public partial class TaskItem : TaskItemBase
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime ModifiedTime { get; set; }
        public DateTime? ResolvedTime { get; set; }
        public bool IsDeleted { get; set; }
    }
}
