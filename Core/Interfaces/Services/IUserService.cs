using Core.Models.Authentication;
using Core.Models.User;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Core.Interfaces.Services
{
    public interface IUserService
    {
        Task<SignInResponse> SilentSignIn(string identifier);
        Task<SignInResponse> SignIn(Credentials credentials);
        Task<bool> SendVerification(string idToken);
        Task<UserProfile> GetProfile(ClaimsPrincipal user);
        Task<UserProfile> UpdateProfile(ClaimsPrincipal user, UserProfile profile, Stream avatar);
    }
}
