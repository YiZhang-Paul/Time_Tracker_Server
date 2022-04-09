using Core.DbContexts;
using Core.Interfaces.Repositories;
using Core.Models.User;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Service.Repositories
{
    public class UserProfileRepository : IUserProfileRepository
    {
        private TimeTrackerDbContext Context { get; }

        public UserProfileRepository(TimeTrackerDbContext context)
        {
            Context = context;
        }

        public async Task<UserProfile> GetProfileById(long id)
        {
            return await Context.UserProfile.FirstOrDefaultAsync(_ => _.Id == id).ConfigureAwait(false);
        }

        public async Task<UserProfile> GetProfileByEmail(string email)
        {
            return await Context.UserProfile.FirstOrDefaultAsync(_ => _.Email == email).ConfigureAwait(false);
        }

        public UserProfile CreateProfile(UserProfile profile)
        {
            profile.CreationTime = DateTime.UtcNow;

            return Context.UserProfile.Add(profile).Entity;
        }
    }
}
