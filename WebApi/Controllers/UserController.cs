using Core.Exceptions.Authentication;
using Core.Interfaces.Services;
using Core.Models.Generic;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Authentication;
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
        public async Task<IActionResult> SignIn([FromBody]Credentials credentials)
        {
            IActionResult result;
            var start = DateTime.UtcNow;

            try
            {
                result = Ok(await UserService.SignIn(credentials).ConfigureAwait(false));
            }
            catch (InvalidCredentialException)
            {
                result = Unauthorized("Invalid login credentials.");
            }
            catch (EmailUnverifiedException)
            {
                result = StatusCode(403, "Must verify email first.");
            }

            var throttle = 3000;
            var elapsed = (int)(DateTime.UtcNow - start).TotalMilliseconds;

            if (elapsed < throttle)
            {
                await Task.Delay(throttle - elapsed).ConfigureAwait(false);
            }

            return result;
        }
    }
}
