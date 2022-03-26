using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Core.Interfaces.Services;
using Core.Models.Authentication;
using Core.Models.Generic;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Text.Json;
using System.Threading.Tasks;

namespace Service.Services
{
    public class UserService : IUserService
    {
        private IConfiguration Configuration { get; }
        private IAmazonSecretsManager Secrets { get; }

        public UserService(IConfiguration configuration, IAmazonSecretsManager secrets)
        {
            Configuration = configuration;
            Secrets = secrets;
        }

        public async Task<TokenResponse> SignIn(Credentials credentials)
        {
            var tokens = await GetTokensByPassword(credentials).ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(tokens.IdToken) || string.IsNullOrWhiteSpace(tokens.AccessToken))
            {
                throw new InvalidCredentialException();
            }

            var emailVerified = GetClaim(tokens.IdToken, "email_verified");

            if (!bool.Parse(emailVerified))
            {
                tokens.AccessToken = null;
            }

            return tokens;
        }

        public async Task<bool> SendVerification(string idToken)
        {
            var userId = GetClaim(idToken, "sub");
            var tokens = await GetTokensByClientCredentials().ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(tokens.AccessToken))
            {
                throw new InvalidCredentialException();
            }

            return await SendVerificationByUserId(userId, tokens.AccessToken).ConfigureAwait(false);
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

        private async Task<TokenResponse> GetTokensByPassword(Credentials credentials)
        {
            var form = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "grant_type", "password" },
                { "audience", Configuration["Auth0:WebAudience"] },
                { "client_id", Configuration["Auth0:WebClientId"] },
                { "client_secret", await GetClientSecret("auth0WebClientSecret").ConfigureAwait(false) },
                { "scope", "openid profile email" },
                { "username", credentials.Email },
                { "password", credentials.Password }
            });

            return await GetTokens(form).ConfigureAwait(false);
        }

        private async Task<TokenResponse> GetTokensByClientCredentials()
        {
            var form = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "grant_type", "client_credentials" },
                { "audience", Configuration["Auth0:MachineAudience"] },
                { "client_id", Configuration["Auth0:MachineClientId"] },
                { "client_secret", await GetClientSecret("auth0MachineClientSecret").ConfigureAwait(false) }
            });

            return await GetTokens(form).ConfigureAwait(false);
        }

        private async Task<TokenResponse> GetTokens(FormUrlEncodedContent form)
        {
            var client = new HttpClient { BaseAddress = new Uri(Configuration["Auth0:Domain"]) };
            var response = await client.PostAsync("oauth/token", form).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidCredentialException();
            }

            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(json))
            {
                throw new InvalidCredentialException();
            }

            return JsonSerializer.Deserialize<TokenResponse>(json);
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

        private async Task<string> GetClientSecret(string key)
        {
            var request = new GetSecretValueRequest { SecretId = Configuration["Aws:SecretId"] };
            var response = await Secrets.GetSecretValueAsync(request).ConfigureAwait(false);

            return JsonSerializer.Deserialize<Dictionary<string, string>>(response.SecretString)[key];
        }
    }
}
