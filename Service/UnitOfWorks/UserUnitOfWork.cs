using Core.DbContexts;
using Core.Interfaces.Repositories;
using Core.Interfaces.UnitOfWorks;
using System.Threading.Tasks;

namespace Service.UnitOfWorks
{
    public class UserUnitOfWork : IUserUnitOfWork
    {
        public IUserProfileRepository UserProfile { get; }
        private TimeTrackerDbContext Context { get; }

        public UserUnitOfWork(TimeTrackerDbContext context, IUserProfileRepository userProfileRepository)
        {
            Context = context;
            UserProfile = userProfileRepository;
        }

        public async Task<bool> Save()
        {
            return await Context.SaveChangesAsync().ConfigureAwait(false) > 0;
        }
    }
}
