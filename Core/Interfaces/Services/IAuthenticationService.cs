using Core.Models.Authentication;
using System.Threading.Tasks;

namespace Core.Interfaces.Services
{
    public interface IAuthenticationService
    {
        Task<FullTokenResponse> GetTokensByPassword(Credentials credentials);
        Task<BaseTokenResponse> GetTokensByClientCredentials();
    }
}
