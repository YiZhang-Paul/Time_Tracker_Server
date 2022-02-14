using Core.Models.WorkItem;
using System.Linq;

namespace Core.Dtos
{
    public class TaskItemSummaryDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public int Effort { get; set; }
        public double Progress { get; set; }

        public static TaskItemSummaryDto Convert(TaskItem item)
        {
            if (item == null)
            {
                return null;
            }

            return new TaskItemSummaryDto
            {
                Id = item.Id,
                Name = item.Name,
                Effort = item.Effort,
                Progress = item.Checklists.Any() ? (double)item.Checklists.Count(_ => _.IsCompleted) / item.Checklists.Count : 0
            };
        }
    }
}
