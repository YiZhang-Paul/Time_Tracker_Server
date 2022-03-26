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
            var response = await GetTokensByPassword(credentials).ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(response))
            {
                throw new InvalidCredentialException();
            }

            var tokens = JsonSerializer.Deserialize<TokenResponse>(response);

            if (string.IsNullOrWhiteSpace(tokens.IdToken) || string.IsNullOrWhiteSpace(tokens.AccessToken))
            {
                throw new InvalidCredentialException();
            }

            if (!IsEmailVerified(tokens.IdToken))
            {
                tokens.AccessToken = null;
            }

            return tokens;
        }

        public async Task<bool> SendVerification(string idToken)
        {
            string userId;

            try
            {
                var parsed = new JwtSecurityTokenHandler().ReadJwtToken(idToken);
                userId = parsed.Claims.First(_ => _.Type == "sub").Value;
            }
            catch
            {
                throw new InvalidCredentialException();
            }

            var json = await GetTokensByClientCredentials().ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(json))
            {
                throw new InvalidCredentialException();
            }

            var tokens = JsonSerializer.Deserialize<TokenResponse>(json);

            if (string.IsNullOrWhiteSpace(tokens.AccessToken))
            {
                throw new InvalidCredentialException();
            }

            var client = new HttpClient { BaseAddress = new Uri(Configuration["Auth0:Domain"]) };
            var request = new HttpRequestMessage(HttpMethod.Post, "api/v2/jobs/verification-email");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);

            request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "user_id", userId },
                { "client_id", Configuration["Auth0:WebClientId"] }
            });

            var response = await client.SendAsync(request).ConfigureAwait(false);

            return response.IsSuccessStatusCode;
        }

        private async Task<string> GetTokensByPassword(Credentials credentials)
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

            var client = new HttpClient { BaseAddress = new Uri(Configuration["Auth0:Domain"]) };
            var response = await client.PostAsync("oauth/token", form).ConfigureAwait(false);

            return response.IsSuccessStatusCode ? await response.Content.ReadAsStringAsync().ConfigureAwait(false) : string.Empty;
        }

        private async Task<string> GetTokensByClientCredentials()
        {
            var form = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "grant_type", "client_credentials" },
                { "audience", Configuration["Auth0:MachineAudience"] },
                { "client_id", Configuration["Auth0:MachineClientId"] },
                { "client_secret", await GetClientSecret("auth0MachineClientSecret").ConfigureAwait(false) }
            });

            var client = new HttpClient { BaseAddress = new Uri(Configuration["Auth0:Domain"]) };
            var response = await client.PostAsync("oauth/token", form).ConfigureAwait(false);

            return response.IsSuccessStatusCode ? await response.Content.ReadAsStringAsync().ConfigureAwait(false) : string.Empty;
        }

        private async Task<string> GetClientSecret(string key)
        {
            var request = new GetSecretValueRequest { SecretId = Configuration["Aws:SecretId"] };
            var response = await Secrets.GetSecretValueAsync(request).ConfigureAwait(false);

            return JsonSerializer.Deserialize<Dictionary<string, string>>(response.SecretString)[key];
        }

        private bool IsEmailVerified(string idToken)
        {
            var parsed = new JwtSecurityTokenHandler().ReadJwtToken(idToken);
            var claim = parsed.Claims.First(_ => _.Type == "email_verified");

            return bool.Parse(claim.Value);
        }
    }
}
