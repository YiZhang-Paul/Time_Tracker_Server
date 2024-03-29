using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Core.Interfaces.Services;
using Core.Interfaces.UnitOfWorks;
using Core.Models.Authentication;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Authentication;
using System.Text.Json;
using System.Threading.Tasks;

namespace Service.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private IConfiguration Configuration { get; }
        private IAmazonSecretsManager Secrets { get; }
        private IUserUnitOfWork UserUnitOfWork { get; }

        public AuthenticationService
        (
            IConfiguration configuration,
            IAmazonSecretsManager secrets,
            IUserUnitOfWork userUnitOfWork
        )
        {
            Configuration = configuration;
            Secrets = secrets;
            UserUnitOfWork = userUnitOfWork;
        }

        public async Task<BaseTokenResponse> GetTokensByRefreshToken(string token)
        {
            var form = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "grant_type", "refresh_token" },
                { "client_id", Configuration["Auth0:WebClientId"] },
                { "client_secret", await GetClientSecret("auth0WebClientSecret").ConfigureAwait(false) },
                { "refresh_token", token }
            });

            return await GetTokens<BaseTokenResponse>(form).ConfigureAwait(false);
        }

        public async Task<FullTokenResponse> GetTokensByPassword(Credentials credentials)
        {
            var form = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "grant_type", "password" },
                { "audience", Configuration["Auth0:WebAudience"] },
                { "client_id", Configuration["Auth0:WebClientId"] },
                { "client_secret", await GetClientSecret("auth0WebClientSecret").ConfigureAwait(false) },
                { "scope", "openid profile email offline_access" },
                { "username", credentials.Email },
                { "password", credentials.Password }
            });

            return await GetTokens<FullTokenResponse>(form).ConfigureAwait(false);
        }

        public async Task<BaseTokenResponse> GetTokensByClientCredentials()
        {
            var form = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "grant_type", "client_credentials" },
                { "audience", Configuration["Auth0:MachineAudience"] },
                { "client_id", Configuration["Auth0:MachineClientId"] },
                { "client_secret", await GetClientSecret("auth0MachineClientSecret").ConfigureAwait(false) }
            });

            return await GetTokens<BaseTokenResponse>(form).ConfigureAwait(false);
        }

        public async Task<UserRefreshToken> RecordRefreshToken(long userId, string token)
        {
            var guid = Guid.NewGuid().ToString();
            var record = await UserUnitOfWork.UserRefreshToken.GetTokenByUserId(userId).ConfigureAwait(false);

            if (record != null)
            {
                record.Guid = guid;
                record.RefreshToken = token;
                record.ExpireTime = DateTime.UtcNow.AddHours(8);
            }
            else
            {
                record = new UserRefreshToken { UserId = userId, Guid = guid, RefreshToken = token };
                UserUnitOfWork.UserRefreshToken.CreateToken(record);
            }

            return await UserUnitOfWork.Save().ConfigureAwait(false) ? record : null;
        }

        public async Task<bool> RevokeRefreshToken(UserRefreshToken record)
        {
            UserUnitOfWork.UserRefreshToken.DeleteToken(record);

            return await UserUnitOfWork.Save().ConfigureAwait(false);
        }

        private async Task<string> GetClientSecret(string key)
        {
            var request = new GetSecretValueRequest { SecretId = Configuration["Aws:SecretId"] };
            var response = await Secrets.GetSecretValueAsync(request).ConfigureAwait(false);

            return JsonSerializer.Deserialize<Dictionary<string, string>>(response.SecretString)[key];
        }

        private async Task<T> GetTokens<T>(FormUrlEncodedContent form)
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

            return JsonSerializer.Deserialize<T>(json);
        }
    }
}
