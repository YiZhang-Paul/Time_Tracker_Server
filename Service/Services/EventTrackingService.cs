using Core.Enums;
using Core.Interfaces.Repositories;
using Core.Models.Event;
using System.Threading.Tasks;

namespace Service.Services
{
    public class EventTrackingService : IEventTrackingService
    {
        private IEventHistoryRepository EventHistoryRepository { get; }

        public EventTrackingService(IEventHistoryRepository eventHistoryRepository)
        {
            EventHistoryRepository = eventHistoryRepository;
        }

        public async Task<bool> StartIdlingSession()
        {
            var last = await EventHistoryRepository.GetLastHistory().ConfigureAwait(false);

            if (last?.EventType == EventType.Idling)
            {
                return false;
            }

            var history = new EventHistory { ResourceId = -1, EventType = EventType.Idling };

            return await EventHistoryRepository.CreateHistory(history).ConfigureAwait(false) != null;
        }
    }
}
