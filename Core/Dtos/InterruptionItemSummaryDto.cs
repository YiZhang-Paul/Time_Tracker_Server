using Core.Enums;

namespace Core.Dtos
{
    public class InterruptionItemSummaryDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public Priority Priority { get; set; }
    }
}
