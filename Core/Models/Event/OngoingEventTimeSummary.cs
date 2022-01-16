namespace Core.Models.Event
{
    public class OngoingEventTimeSummary
    {
        public EventTimeSummary ConcludedSinceStart { get; set; } = new EventTimeSummary();
        public EventTimeSummary ConcludedSinceLastBreakPrompt { get; set; } = new EventTimeSummary();
        public EventHistory UnconcludedSinceStart { get; set; }
        public EventHistory UnconcludedSinceLastBreakPrompt { get; set; }
    }
}
