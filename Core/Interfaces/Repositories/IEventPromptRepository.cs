using Core.Enums;
using Core.Models.Event;
using System.Threading.Tasks;

namespace Core.Interfaces.Repositories
{
    public interface IEventPromptRepository
    {
        Task<EventPrompt> GetLastEventPrompt(PromptType? type);
        Task<EventPrompt> CreateEventPrompt(EventPrompt prompt);
    }
}
