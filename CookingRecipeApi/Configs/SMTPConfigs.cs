namespace CookingRecipeApi.Configs
{
#pragma warning disable CS8618
    public class SMTPConfigs
    {
        public string SMTPServer { get; set; }
        public int Port { get; set; }
        public string SecurityChoice { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string FromEmail { get; set; }
    }
}
