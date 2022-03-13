using Core.DbContexts;
using Core.Enums;
using Core.Interfaces.Repositories;
using Core.Models.Event;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Service.Repositories
{
    public class EventPromptRepository : IEventPromptRepository
    {
        private TimeTrackerDbContext Context { get; }

        public EventPromptRepository(TimeTrackerDbContext context)
        {
            Context = context;
        }

        public async Task<EventPrompt> GetLastPrompt(PromptType? type)
        {
            var sorted = Context.EventPrompt.OrderByDescending(_ => _.Id);

            if (!type.HasValue)
            {
                return await sorted.FirstOrDefaultAsync().ConfigureAwait(false);
            }

            return await sorted.FirstOrDefaultAsync(_ => _.PromptType == type).ConfigureAwait(false);
        }

        public EventPrompt CreatePrompt(EventPrompt prompt)
        {
            prompt.Timestamp = DateTime.UtcNow;

            return Context.EventPrompt.Add(prompt).Entity;
        }
    }
}
