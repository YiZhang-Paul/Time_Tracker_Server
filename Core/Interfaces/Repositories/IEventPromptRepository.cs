using Core.Enums;
using Core.Models.Event;
using System.Threading.Tasks;

namespace Core.Interfaces.Repositories
{
    public interface IEventPromptRepository
    {
        Task<EventPrompt> GetLastPrompt(long userId, PromptType? type);
        EventPrompt CreatePrompt(long userId, EventPrompt prompt);
    }
}
