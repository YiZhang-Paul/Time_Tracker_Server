using Core.Models.User;

namespace Core.Models.Authentication
{
    public class SignInResponse
    {
        public string VerificationSequence { get; set; }
        public BaseTokenResponse Tokens { get; set; }
        public UserProfile Profile { get; set; }
    }
}
