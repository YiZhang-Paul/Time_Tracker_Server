namespace Core.Models.Event
{
    public class OngoingEventTimeSummary
    {
        public EventTimeSummary SinceStart { get; set; } = new EventTimeSummary();
        public EventTimeSummary SinceLastBreakPrompt { get; set; } = new EventTimeSummary();
        public EventHistory Unconcluded { get; set; }
    }
}
