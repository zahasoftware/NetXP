namespace NetXP.NetStandard.Network.Email
{
    public class MailCredential
    {
        public MailCredential()
        {
        }

        public MailCredential(string user, string password)
        {
            this.User = user;
            this.Password = password;
        }

        public string Password { get; internal set; }
        public string User { get; internal set; }
    }
}