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

        public static EventDurationDto Convert(EventTimelineDto timeline, int duration)
        {
            if (timeline == null)
            {
                return null;
            }

            return new EventDurationDto
            {
                Id = timeline.Id,
                EventType = timeline.EventType,
                Duration = duration,
                Name = timeline.Name,
                IsDeleted = timeline.IsDeleted
            };
        }
    }
}
