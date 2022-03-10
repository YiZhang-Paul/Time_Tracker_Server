using Core.Enums;
using System;

namespace Core.Dtos
{
    public class EventTimeRangeDto
    {
        public long Id { get; set; }
        public EventType EventType { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }
}
