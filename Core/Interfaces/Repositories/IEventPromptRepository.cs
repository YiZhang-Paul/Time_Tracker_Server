using Core.Enums;
using Core.Models.Event;
using System.Threading.Tasks;

namespace Core.Interfaces.Repositories
{
    public interface IEventPromptRepository
    {
        Task<EventPrompt> GetLastPrompt(PromptType? type);
        EventPrompt CreatePrompt(EventPrompt prompt);
    }
}
