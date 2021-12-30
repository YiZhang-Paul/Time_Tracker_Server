using Core.Dtos;
using Core.Models.Interruption;
using System.Threading.Tasks;

namespace Core.Interfaces.Repositories
{
    public interface IInterruptionItemRepository
    {
        Task<InterruptionItem> CreateInterruptionItem(InterruptionItemCreationDto item);
    }
}
