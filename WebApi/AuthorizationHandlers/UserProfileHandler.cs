using Core.Interfaces.UnitOfWorks;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Threading.Tasks;
using WebApi.AuthorizationRequirements;

namespace WebApi.AuthorizationHandlers
{
    public class UserProfileHandler : AuthorizationHandler<UserProfileRequirement>
    {
        private IUserUnitOfWork UserUnitOfWork { get; }

        public UserProfileHandler(IUserUnitOfWork userUnitOfWork)
        {
            UserUnitOfWork = userUnitOfWork;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, UserProfileRequirement requirement)
        {
            var claim = context.User.Claims.FirstOrDefault(_ => _.Type == "https://ticking/email");
            var email = claim?.Value ?? string.Empty;

            if (string.IsNullOrWhiteSpace(email))
            {
                return;
            }

            if (await UserUnitOfWork.UserProfile.GetProfileByEmail(email).ConfigureAwait(false) != null)
            {
                context.Succeed(requirement);
            }

            return;
        }
    }
}
