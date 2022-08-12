namespace NetXP.Network
{
    public class SLJPOption
    {
        public SLJPOption()
        {
            this.MaxOfBytesToReceive = int.MaxValue / 2 / 2;
            this.MaxOfBytesToReceive = int.MaxValue / 2 / 2;
        }

        public int MaxOfBytesToReceive { get; set; }
        public int MaxOfBytesToSend { get; set; }

        public double SecurityMaxSizeToReceive { get; set; }
    }
}