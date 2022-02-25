using Core.Enums;
using Core.Models.Event;
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
        public bool IsResolved { get; set; }

        public static EventTimelineDto Convert(EventHistorySummary summary)
        {
            if (summary == null)
            {
                return null;
            }

            return new EventTimelineDto
            {
                Id = summary.ResourceId,
                EventType = summary.EventType,
                StartTime = summary.Timestamp,
                Name = summary.Name,
                IsDeleted = summary.IsDeleted,
                IsResolved = summary.IsResolved
            };
        }
    }
}
