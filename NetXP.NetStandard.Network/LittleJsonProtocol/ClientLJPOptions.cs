namespace NetXP.NetStandard.Network.LittleJsonProtocol
{
    public class ClientLJPOptions
    {
        /// <summary>
        /// Expressed in seconds
        /// </summary>
        public int ExpirationDNSCache { get; set; } = 60 * 5;//5 Min
    }
}