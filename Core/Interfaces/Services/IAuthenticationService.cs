using Core.Models.Authentication;
using System.Threading.Tasks;

namespace Core.Interfaces.Services
{
    public interface IAuthenticationService
    {
        Task<TokenResponse> GetTokensByPassword(Credentials credentials);
        Task<TokenResponse> GetTokensByClientCredentials();
    }
}
