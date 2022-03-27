using Core.Interfaces.Services;
using Core.Models.Authentication;
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
                var response = await UserService.SignIn(credentials).ConfigureAwait(false);
                result = string.IsNullOrWhiteSpace(response.Tokens.AccessToken) ? StatusCode(403, response) : Ok(response);
            }
            catch (InvalidCredentialException)
            {
                result = Unauthorized(null);
            }

            var throttle = 3000;
            var elapsed = (int)(DateTime.UtcNow - start).TotalMilliseconds;

            if (elapsed < throttle)
            {
                await Task.Delay(throttle - elapsed).ConfigureAwait(false);
            }

            return result;
        }

        [HttpPost]
        [Route("verification")]
        public async Task<bool> SendVerification([FromBody]string idToken)
        {
            return await UserService.SendVerification(idToken).ConfigureAwait(false);
        }
    }
}
