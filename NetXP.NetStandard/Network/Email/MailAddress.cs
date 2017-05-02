namespace NetXP.NetStandard.Network.Email
{
    public class MailAddress
    {
        public MailAddress(string address, string alias)
        {
            this.Alias = alias;
            this.Address = address;
        }

        public string Alias { get; private set; }
        public string Address { get; private set; }
    }
}