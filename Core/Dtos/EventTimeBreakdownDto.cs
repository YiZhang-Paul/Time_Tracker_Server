namespace Core.Dtos
{
    public class EventTimeBreakdownDto
    {
        public int Idling { get; set; }
        public int Break { get; set; }
        public int Interruption { get; set; }
        public int Task { get; set; }
    }
}