using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Core.Exceptions.Authentication;
using Core.Interfaces.Services;
using Core.Models.Authentication;
using Core.Models.Generic;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
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

        public async Task<string> SignIn(Credentials credentials)
        {
            try
            {
                var response = await GetTokens(credentials).ConfigureAwait(false);

                if (string.IsNullOrWhiteSpace(response))
                {
                    throw new InvalidCredentialException();
                }

                var tokens = JsonSerializer.Deserialize<TokenResponse>(response);

                if (!IsEmailVerified(tokens.IdToken))
                {
                    throw new EmailUnverifiedException();
                }

                return tokens.AccessToken;
            }
            catch
            {
                throw new InvalidCredentialException();
            }
        }

        private async Task<string> GetTokens(Credentials credentials)
        {
            var form = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "grant_type", "password" },
                { "audience", Configuration["Auth0:Audience"] },
                { "client_id", Configuration["Auth0:ClientId"] },
                { "client_secret", await GetClientSecret().ConfigureAwait(false) },
                { "scope", "openid profile email" },
                { "username", credentials.Email },
                { "password", credentials.Password }
            });

            var client = new HttpClient { BaseAddress = new Uri(Configuration["Auth0:Domain"]) };
            var response = await client.PostAsync("oauth/token", form).ConfigureAwait(false);

            return response.IsSuccessStatusCode ? await response.Content.ReadAsStringAsync().ConfigureAwait(false) : string.Empty;
        }

        private async Task<string> GetClientSecret()
        {
            var request = new GetSecretValueRequest { SecretId = Configuration["Aws:SecretId"] };
            var response = await Secrets.GetSecretValueAsync(request).ConfigureAwait(false);
            var secrets = JsonSerializer.Deserialize<Dictionary<string, string>>(response.SecretString);

            return secrets["auth0ClientSecret"];
        }

        private bool IsEmailVerified(string idToken)
        {
            var parsed = new JwtSecurityTokenHandler().ReadJwtToken(idToken);
            var claim = parsed.Claims.First(_ => _.Type == "email_verified");

            return bool.Parse(claim.Value);
        }
    }
}
