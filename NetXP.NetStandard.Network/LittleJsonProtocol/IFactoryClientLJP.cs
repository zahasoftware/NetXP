namespace NetXP.NetStandard.Network.LittleJsonProtocol
{
    public interface IFactoryClientLJP
    {
        IClientLJP Create();

        ILJPMessageFactory CreateMessageFactory(string version);
    }
}
