namespace NetXP.NetStandard.Network.Email
{
    public class MailServer
    {
        public MailServer(string server, int port, string user, string password)
        {
            this.Server = server;
            this.Port = port;
            this.Credential = new MailCredential(user, password);
        }

        public MailCredential Credential { get; internal set; }
        public int Port { get; internal set; }
        public string Server { get; internal set; }
    }
}