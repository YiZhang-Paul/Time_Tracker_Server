using Core.Enums;
using System;

namespace Core.Dtos
{
    public class EventTimelineDto
    {
        public long Id { get; set; }
        public EventType EventType { get; set; }
        public DateTime StartTime { get; set; }
        public string Name { get; set; }
        public bool IsDeleted { get; set; }
    }
}
