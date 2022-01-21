using Core.Models.Event;

namespace Core.Dtos
{
    public class OngoingEventTimeSummaryDto
    {
        public EventTimeSummary ConcludedSinceStart { get; set; } = new EventTimeSummary();
        public EventTimeSummary ConcludedSinceLastBreakPrompt { get; set; } = new EventTimeSummary();
        public EventHistory UnconcludedSinceStart { get; set; }
        public EventHistory UnconcludedSinceLastBreakPrompt { get; set; }
    }
}
