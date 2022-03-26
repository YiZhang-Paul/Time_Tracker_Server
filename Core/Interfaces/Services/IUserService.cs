using Core.Models.Authentication;
using Core.Models.Generic;
using System.Threading.Tasks;

namespace Core.Interfaces.Services
{
    public interface IUserService
    {
        Task<TokenResponse> SignIn(Credentials credentials);
    }
}
