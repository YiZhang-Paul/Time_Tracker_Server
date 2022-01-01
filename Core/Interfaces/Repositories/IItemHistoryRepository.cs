using Core.Models.ItemHistory;
using System.Threading.Tasks;

namespace Core.Interfaces.Repositories
{
    public interface IItemHistoryRepository
    {
        Task<ItemHistory> GetLastItemHistory();
        Task<ItemHistory> CreateItemHistory(ItemHistory history);
    }
}
