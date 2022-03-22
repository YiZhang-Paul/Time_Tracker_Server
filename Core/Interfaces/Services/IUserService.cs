using Core.Models.Generic;
using System.Threading.Tasks;

namespace Core.Interfaces.Services
{
    public interface IUserService
    {
        Task<string> SignIn(Credentials credentials);
    }
}
