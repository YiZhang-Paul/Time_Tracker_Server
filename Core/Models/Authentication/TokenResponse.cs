using System.Text.Json.Serialization;

namespace Core.Models.Authentication
{
    public class TokenResponse
    {
        [JsonPropertyName("id_token")]
        public string IdToken { get; set; }
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }
    }
}
