using Core.Enums;
using Core.Models.Generic;
using System.Collections.Generic;

namespace Core.Dtos
{
    public class EventDurationDto
    {
        public long Id { get; set; }
        public EventType EventType { get; set; }
        public int Duration { get; set; }
        public string Name { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsResolved { get; set; }
        public List<TimePeriod> Periods { get; set; } = new List<TimePeriod>();

        public static EventDurationDto Convert(EventTimelineDto timeline)
        {
            if (timeline == null)
            {
                return null;
            }

            return new EventDurationDto
            {
                Id = timeline.Id,
                EventType = timeline.EventType,
                Name = timeline.Name,
                IsDeleted = timeline.IsDeleted,
                IsResolved = timeline.IsResolved
            };
        }
    }
}
