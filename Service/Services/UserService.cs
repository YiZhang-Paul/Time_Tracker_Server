using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Core.Interfaces.Services;
using Core.Models.Generic;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
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

                if (!response.IsSuccessStatusCode)
                {
                    return string.Empty;
                }

                return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
            catch
            {
                return string.Empty;
            }
        }

        private async Task<string> GetClientSecret()
        {
            var request = new GetSecretValueRequest { SecretId = Configuration["Aws:SecretId"] };
            var response = await Secrets.GetSecretValueAsync(request).ConfigureAwait(false);
            var secrets = JsonSerializer.Deserialize<Dictionary<string, string>>(response.SecretString);

            return secrets["auth0ClientSecret"];
        }
    }
}
