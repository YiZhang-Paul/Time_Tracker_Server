using System.Collections.Generic;

namespace Core.Dtos
{
    public class EventSummariesDto
    {
        public List<EventTimelineDto> Timeline { get; set; } = new List<EventTimelineDto>();
        public List<EventDurationDto> Idling { get; set; } = new List<EventDurationDto>();
        public List<EventDurationDto> Break { get; set; } = new List<EventDurationDto>();
        public List<EventDurationDto> Interruption { get; set; } = new List<EventDurationDto>();
        public List<EventDurationDto> Task { get; set; } = new List<EventDurationDto>();
    }
}
