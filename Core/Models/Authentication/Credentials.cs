namespace Core.Models.Authentication
{
    public class Credentials
    {
        public string Email { get => _email; set => _email = value.Trim(); }
        public string Password { get; set; }
        private string _email = string.Empty;
    }
}
