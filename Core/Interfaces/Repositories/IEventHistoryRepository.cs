using Core.Models.EventHistory;
using System.Threading.Tasks;

namespace Core.Interfaces.Repositories
{
    public interface IEventHistoryRepository
    {
        Task<EventHistory> GetLastEventHistory();
        Task<EventHistory> CreateEventHistory(EventHistory history);
    }
}
