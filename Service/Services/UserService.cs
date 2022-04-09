using Core.Interfaces.Services;
using Core.Interfaces.UnitOfWorks;
using Core.Models.Authentication;
using Core.Models.User;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Service.Services
{
    public class UserService : IUserService
    {
        private IConfiguration Configuration { get; }
        private IUserUnitOfWork UserUnitOfWork { get; }
        private IAuthenticationService AuthenticationService { get; }

        public UserService
        (
            IConfiguration configuration,
            IUserUnitOfWork userUnitOfWork,
            IAuthenticationService authenticationService
        )
        {
            Configuration = configuration;
            UserUnitOfWork = userUnitOfWork;
            AuthenticationService = authenticationService;
        }

        public async Task<SignInResponse> SilentSignIn(string identifier)
        {
            var refreshToken = await GetUserRefreshToken(identifier).ConfigureAwait(false);

            if (refreshToken == null)
            {
                throw new InvalidCredentialException();
            }

            if (refreshToken.ExpireTime <= DateTime.UtcNow)
            {
                await AuthenticationService.RevokeRefreshToken(refreshToken).ConfigureAwait(false);

                throw new InvalidCredentialException();
            }

            var tokens = await AuthenticationService.GetTokensByRefreshToken(refreshToken.RefreshToken).ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(tokens.IdToken) || string.IsNullOrWhiteSpace(tokens.AccessToken))
            {
                await AuthenticationService.RevokeRefreshToken(refreshToken).ConfigureAwait(false);

                throw new InvalidCredentialException();
            }

            var profile = await UserUnitOfWork.UserProfile.GetProfileById(refreshToken.UserId).ConfigureAwait(false);

            if (profile == null)
            {
                throw new InvalidOperationException();
            }

            return new SignInResponse
            {
                VerificationSequence = refreshToken.Guid,
                Tokens = new BaseTokenResponse { IdToken = tokens.IdToken, AccessToken = tokens.AccessToken },
                Profile = profile
            };
        }

        public async Task<SignInResponse> SignIn(Credentials credentials)
        {
            var tokens = await AuthenticationService.GetTokensByPassword(credentials).ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(tokens.IdToken) || string.IsNullOrWhiteSpace(tokens.AccessToken) || string.IsNullOrWhiteSpace(tokens.RefreshToken))
            {
                throw new InvalidCredentialException();
            }

            var emailVerified = GetClaim(tokens.IdToken, "email_verified");

            if (!bool.Parse(emailVerified))
            {
                return new SignInResponse
                {
                    Tokens = new BaseTokenResponse { IdToken = tokens.IdToken }
                };
            }

            var profile = await EnsureProfileCreation(credentials.Email).ConfigureAwait(false);

            if (profile == null)
            {
                throw new InvalidOperationException();
            }

            var refreshToken = await AuthenticationService.RecordRefreshToken(profile.Id, tokens.RefreshToken).ConfigureAwait(false);

            return new SignInResponse
            {
                VerificationSequence = refreshToken.Guid,
                Tokens = new BaseTokenResponse { IdToken = tokens.IdToken, AccessToken = tokens.AccessToken },
                Profile = profile
            };
        }

        public async Task<bool> SendVerification(string idToken)
        {
            var userId = GetClaim(idToken, "sub");
            var tokens = await AuthenticationService.GetTokensByClientCredentials().ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(tokens.AccessToken))
            {
                throw new InvalidCredentialException();
            }

            return await SendVerificationByUserId(userId, tokens.AccessToken).ConfigureAwait(false);
        }

        public async Task<UserProfile> GetProfile(ClaimsPrincipal user)
        {
            var claim = user.Claims.FirstOrDefault(_ => _.Type == "https://ticking/email");
            var email = claim?.Value ?? string.Empty;

            return string.IsNullOrWhiteSpace(email) ? null : await UserUnitOfWork.UserProfile.GetProfileByEmail(email).ConfigureAwait(false);
        }

        public async Task<UserProfile> UpdateProfile(ClaimsPrincipal user, UserProfile profile)
        {
            var existing = await GetProfile(user).ConfigureAwait(false);

            if (existing == null)
            {
                return null;
            }

            try
            {
                existing.DisplayName = profile.DisplayName;
                existing.TimeSessionOptions = profile.TimeSessionOptions;
                await UserUnitOfWork.Save().ConfigureAwait(false);

                return existing;
            }
            catch
            {
                return null;
            }
        }

        private async Task<UserRefreshToken> GetUserRefreshToken(string identifier)
        {
            if (!Regex.IsMatch(identifier, @"^\d+\|[0-9a-zA-Z-]+$"))
            {
                return null;
            }

            var segments = identifier.Split('|');

            if (!long.TryParse(segments[0], out var userId))
            {
                return null;
            }

            return await UserUnitOfWork.UserRefreshToken.GetToken(userId, segments[1]).ConfigureAwait(false);
        }

        private async Task<UserProfile> EnsureProfileCreation(string email)
        {
            var profile = await UserUnitOfWork.UserProfile.GetProfileByEmail(email).ConfigureAwait(false);

            if (profile != null)
            {
                return profile;
            }

            var created = new UserProfile { Email = email, DisplayName = $"user{DateTime.UtcNow:yyyyMMdd}" };
            UserUnitOfWork.UserProfile.CreateProfile(created);

            return await UserUnitOfWork.Save().ConfigureAwait(false) ? created : null;
        }

        private async Task<bool> SendVerificationByUserId(string userId, string accessToken)
        {
            var client = new HttpClient { BaseAddress = new Uri(Configuration["Auth0:Domain"]) };
            var request = new HttpRequestMessage(HttpMethod.Post, "api/v2/jobs/verification-email");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "user_id", userId },
                { "client_id", Configuration["Auth0:WebClientId"] }
            });

            var response = await client.SendAsync(request).ConfigureAwait(false);

            return response.IsSuccessStatusCode;
        }

        private string GetClaim(string token, string type)
        {
            try
            {
                var claims = new JwtSecurityTokenHandler().ReadJwtToken(token).Claims;

                return claims.First(_ => _.Type == type).Value;
            }
            catch
            {
                throw new InvalidCredentialException();
            }
        }
    }
}
