using System;

namespace Core.Models.User
{
    public class UserProfile
    {
        public long Id { get; set; }
        public string Email { get; set; }
        public string DisplayName { get; set; }
        public string AvatarUrl { get; set; }
        public DateTime CreationTime { get; set; }
        public TimeSessionOptions TimeSessionOptions { get; set; } = new TimeSessionOptions();
    }
}
