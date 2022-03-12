using Core.Enums;
using System;

namespace Core.Models.Event
{
    public class EventHistorySummary
    {
        public long Id { get; set; }
        public long ResourceId { get; set; }
        public EventType EventType { get; set; }
        public DateTime Timestamp { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsDeleted { get; set; }
        public bool IsResolved { get; set; }
    }
}
