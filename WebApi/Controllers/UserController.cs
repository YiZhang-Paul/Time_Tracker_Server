using Core.Interfaces.Services;
using Core.Models.Authentication;
using Core.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Authentication;
using System.Text.Json;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [Route("api/v1/users")]
    [Authorize(Policy = "UserProfile")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private IUserService UserService { get; }

        public UserController(IUserService userService)
        {
            UserService = userService;
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("silent-sign-in")]
        public async Task<IActionResult> SilentSignIn([FromBody]string identifier)
        {
            try
            {
                return Ok(await UserService.SilentSignIn(identifier).ConfigureAwait(false));
            }
            catch (InvalidCredentialException)
            {
                return Unauthorized(null);
            }
        }

        [HttpPost]
        [AllowAnonymous]
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
        [AllowAnonymous]
        [Route("verification")]
        public async Task<bool> SendVerification([FromBody]string idToken)
        {
            return await UserService.SendVerification(idToken).ConfigureAwait(false);
        }

        [HttpPut]
        [Route("profile")]
        public async Task<UserProfile> UpdateProfile([FromForm]IFormCollection data)
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var profile = JsonSerializer.Deserialize<UserProfile>(data["profile"], options);
            var avatar = data.Files.Count == 1 ? data.Files[0].OpenReadStream() : null;

            return await UserService.UpdateProfile(HttpContext.User, profile, avatar).ConfigureAwait(false);
        }
    }
}
