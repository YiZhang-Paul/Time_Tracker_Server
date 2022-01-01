using Core.Enums;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;
using Core.Models.Interruption;
using Core.Models.ItemHistory;
using Core.Models.Task;
using System.Threading.Tasks;

namespace Service.Services
{
    public class ItemHistoryService : IItemHistoryService
    {
        private IItemHistoryRepository ItemHistoryRepository { get; }

        public ItemHistoryService(IItemHistoryRepository itemHistoryRepository)
        {
            ItemHistoryRepository = itemHistoryRepository;
        }

        public async Task<bool> StartIdlingSession()
        {
            var last = await ItemHistoryRepository.GetLastItemHistory().ConfigureAwait(false);

            if (last?.ItemType == ItemType.Idling)
            {
                return false;
            }

            var history = new ItemHistory { ItemId = -1, ItemType = ItemType.Idling };

            return await ItemHistoryRepository.CreateItemHistory(history).ConfigureAwait(false) != null;
        }

        public async Task<bool> StartInterruptionItem(InterruptionItem item)
        {
            var last = await ItemHistoryRepository.GetLastItemHistory().ConfigureAwait(false);

            if (last?.ItemType == ItemType.Interruption && last?.ItemId == item.Id)
            {
                return false;
            }

            var history = new ItemHistory { ItemId = item.Id, ItemType = ItemType.Interruption };

            return await ItemHistoryRepository.CreateItemHistory(history).ConfigureAwait(false) != null;
        }

        public async Task<bool> StartTaskItem(TaskItem item)
        {
            var last = await ItemHistoryRepository.GetLastItemHistory().ConfigureAwait(false);

            if (last?.ItemType == ItemType.Task && last?.ItemId == item.Id)
            {
                return false;
            }

            var history = new ItemHistory { ItemId = item.Id, ItemType = ItemType.Task };

            return await ItemHistoryRepository.CreateItemHistory(history).ConfigureAwait(false) != null;
        }
    }
}
