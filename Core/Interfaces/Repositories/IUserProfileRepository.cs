using Core.Models.Authentication;
using System.Threading.Tasks;

namespace Core.Interfaces.Repositories
{
    public interface IUserProfileRepository
    {
        Task<UserProfile> GetProfileById(long id);
        UserProfile CreateProfile(UserProfile profile);
    }
}
