using System.Collections.Generic;

namespace Core.Dtos
{
    public class ItemSummariesDto
    {
        public List<InterruptionItemSummaryDto> Resolved { get; set; } = new List<InterruptionItemSummaryDto>();
        public List<InterruptionItemSummaryDto> Unresolved { get; set; } = new List<InterruptionItemSummaryDto>();
    }
}
