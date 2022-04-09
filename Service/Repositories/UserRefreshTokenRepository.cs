using Core.DbContexts;
using Core.Interfaces.Repositories;
using Core.Models.Authentication;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Service.Repositories
{
    public class UserRefreshTokenRepository : IUserRefreshTokenRepository
    {
        private TimeTrackerDbContext Context { get; }

        public UserRefreshTokenRepository(TimeTrackerDbContext context)
        {
            Context = context;
        }

        public async Task<UserRefreshToken> GetTokenByUserId(long userId)
        {
            return await Context.UserRefreshToken.FirstOrDefaultAsync(_ => _.UserId == userId).ConfigureAwait(false);
        }

        public async Task<UserRefreshToken> GetToken(long userId, string guid)
        {
            return await Context.UserRefreshToken.FirstOrDefaultAsync(_ => _.UserId == userId && _.Guid == guid).ConfigureAwait(false);
        }

        public UserRefreshToken CreateToken(UserRefreshToken record)
        {
            return Context.UserRefreshToken.Add(record).Entity;
        }

        public void DeleteToken(UserRefreshToken record)
        {
            Context.UserRefreshToken.Remove(record);
        }
    }
}
