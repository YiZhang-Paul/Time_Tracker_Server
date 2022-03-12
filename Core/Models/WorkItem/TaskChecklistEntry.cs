namespace Core.Models.WorkItem
{
    public class TaskChecklistEntry
    {
        public long Id { get; set; }
        public string Description { get; set; }
        public string Rank { get; set; }
        public bool IsCompleted { get; set; }
    }
}
