using Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using WebApi.AuthorizationRequirements;

namespace WebApi.AuthorizationHandlers
{
    public class UserProfileHandler : AuthorizationHandler<UserProfileRequirement>
    {
        private IUserService UserService { get; }

        public UserProfileHandler(IUserService userService)
        {
            UserService = userService;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, UserProfileRequirement requirement)
        {
            if (await UserService.GetProfile(context.User).ConfigureAwait(false) != null)
            {
                context.Succeed(requirement);
            }

            return;
        }
    }
}
