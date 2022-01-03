using Core.Enums;
using Core.Models.EventHistory;
using System.Threading.Tasks;

namespace Core.Interfaces.Repositories
{
    public interface IEventPromptRepository
    {
        Task<EventPrompt> GetLastEventPrompt(PromptType? type);
    }
}
