using Core.Models.Authentication;
using System.Threading.Tasks;

namespace Core.Interfaces.Services
{
    public interface IAuthenticationService
    {
        Task<BaseTokenResponse> GetTokensByRefreshToken(string token);
        Task<FullTokenResponse> GetTokensByPassword(Credentials credentials);
        Task<BaseTokenResponse> GetTokensByClientCredentials();
        Task<UserRefreshToken> RecordRefreshToken(long userId, string token);
        Task<bool> RevokeRefreshToken(UserRefreshToken record);
    }
}
