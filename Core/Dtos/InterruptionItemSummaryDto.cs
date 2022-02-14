using Core.Enums;
using Core.Models.WorkItem;
using System.Linq;

namespace Core.Dtos
{
    public class InterruptionItemSummaryDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public Priority Priority { get; set; }
        public double Progress { get; set; }

        public static InterruptionItemSummaryDto Convert(InterruptionItem item)
        {
            if (item == null)
            {
                return null;
            }

            return new InterruptionItemSummaryDto
            {
                Id = item.Id,
                Name = item.Name,
                Priority = item.Priority,
                Progress = item.Checklists.Any() ? (double)item.Checklists.Count(_ => _.IsCompleted) / item.Checklists.Count : 0
            };
        }
    }
}
