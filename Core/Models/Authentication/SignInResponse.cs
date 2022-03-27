namespace Core.Models.Authentication
{
    public class SignInResponse
    {
        public BaseTokenResponse Tokens { get; set; }
        public UserProfile Profile { get; set; }
    }
}
