using Core.Models.Authentication;
using System.Threading.Tasks;

namespace Core.Interfaces.Repositories
{
    public interface IUserRefreshTokenRepository
    {
        Task<UserRefreshToken> GetTokenByUserId(long userId);
        UserRefreshToken CreateToken(UserRefreshToken record);
        void DeleteToken(UserRefreshToken record);
    }
}
