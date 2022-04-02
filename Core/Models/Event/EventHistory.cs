using Core.Enums;
using System;

namespace Core.Models.Event
{
    public class EventHistory
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public long ResourceId { get; set; }
        public EventType EventType { get; set; }
        public int TargetDuration { get; set; } = -1;
        public DateTime Timestamp { get; set; }
    }
}
