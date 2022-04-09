using Core.Models.User;
using System.Threading.Tasks;

namespace Core.Interfaces.Repositories
{
    public interface IUserProfileRepository
    {
        Task<UserProfile> GetProfileById(long id);
        Task<UserProfile> GetProfileByEmail(string email);
        UserProfile CreateProfile(UserProfile profile);
    }
}
