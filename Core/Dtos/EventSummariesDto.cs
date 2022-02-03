using System.Collections.Generic;

namespace Core.Dtos
{
    public class EventSummariesDto
    {
        public List<EventTimelineDto> Timeline { get; set; } = new List<EventTimelineDto>();
        public List<EventDurationDto> Duration { get; set; } = new List<EventDurationDto>();
    }
}
