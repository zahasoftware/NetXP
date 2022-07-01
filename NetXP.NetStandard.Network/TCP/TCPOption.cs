namespace NetXP.Network
{
    public class TCPOption
    {
        public TCPOption()
        {
            ReceiveTimeOut = 1000 * 30;
        }

        public int ReceiveTimeOut { get; set; }
    }
}