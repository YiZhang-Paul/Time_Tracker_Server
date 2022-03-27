using System;

namespace Core.Models.Authentication
{
    public class UserRefreshToken
    {
        public long UserId { get; set; }
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpireTime { get; set; } = DateTime.UtcNow.AddHours(8);
    }
}
