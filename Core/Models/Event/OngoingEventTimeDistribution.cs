namespace Core.Models.Event
{
    public class OngoingEventTimeDistribution
    {
        public EventTimeDistribution SinceStart { get; set; } = new EventTimeDistribution();
        public EventTimeDistribution SinceLastBreakPrompt { get; set; } = new EventTimeDistribution();
        public EventHistory Unconcluded { get; set; }
    }
}
