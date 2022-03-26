namespace Core.Models.Authentication
{
    public class SignInResponse
    {
        public TokenResponse Tokens { get; set; }
        public UserProfile Profile { get; set; }
    }
}
