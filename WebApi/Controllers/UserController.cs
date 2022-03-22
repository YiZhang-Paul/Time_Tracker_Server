using Core.Interfaces.Services;
using Core.Models.Generic;
using Microsoft.AspNetCore.Mvc;
using System;
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
            var start = DateTime.UtcNow;
            var throttle = 3000;
            var response = await UserService.SignIn(credentials).ConfigureAwait(false);
            var elapsed = (int)(DateTime.UtcNow - start).TotalMilliseconds;

            if (elapsed < throttle)
            {
                await Task.Delay(throttle - elapsed).ConfigureAwait(false);
            }

            return response;
        }
    }
}
