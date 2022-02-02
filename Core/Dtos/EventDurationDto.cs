using Core.Enums;

namespace Core.Dtos
{
    public class EventDurationDto
    {
        public long Id { get; set; }
        public EventType EventType { get; set; }
        public int Duration { get; set; }
        public string Name { get; set; }
        public bool IsDeleted { get; set; }
    }
}
