namespace Core.Models.EventHistory
{
    public class EventTimeDistribution
    {
        public int Idling { get; set; }
        public int Interruption { get; set; }
        public int Task { get; set; }
        public EventHistory Unconcluded { get; set; }
    }
}
