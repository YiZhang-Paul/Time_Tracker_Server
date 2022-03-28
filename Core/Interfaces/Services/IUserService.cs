using Core.Models.Authentication;
using System.Threading.Tasks;

namespace Core.Interfaces.Services
{
    public interface IUserService
    {
        Task<SignInResponse> SilentSignIn(long userId);
        Task<SignInResponse> SignIn(Credentials credentials);
        Task<bool> SendVerification(string idToken);
    }
}
