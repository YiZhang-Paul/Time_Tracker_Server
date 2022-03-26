using System;

namespace Core.Models.Authentication
{
    public class UserProfile
    {
        public long Id { get; set; }
        public string Email { get; set; }
        public string DisplayName { get; set; }
        public DateTime CreationTime { get; set; }
    }
}
