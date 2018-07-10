namespace NetXP.NetStandard.Network
{
    public class TCPOption
    {
        public TCPOption()
        {
            ReceiveTimeOut = 1000 * 10;
        }

        public int ReceiveTimeOut { get; set; }
    }
}