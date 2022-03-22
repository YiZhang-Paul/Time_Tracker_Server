using Core.Interfaces.Services;
using Core.Models.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [Route("api/v1/users")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private IUserService UserService { get; }

        public UserController(IUserService userService)
        {
            UserService = userService;
        }

        [HttpPost]
        [Route("sign-in")]
        public async Task<string> SignIn([FromBody]Credentials credentials)
        {
            return await UserService.SignIn(credentials).ConfigureAwait(false);
        }
    }
}
