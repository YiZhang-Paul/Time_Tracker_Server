using System.Text.Json.Serialization;

namespace Core.Models.Authentication
{
    public class FullTokenResponse : BaseTokenResponse
    {
        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; }
    }
}
