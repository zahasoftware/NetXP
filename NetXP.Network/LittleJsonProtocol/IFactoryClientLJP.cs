namespace NetXP.Network.LittleJsonProtocol
{
    public interface IFactoryClientLJP
    {
        IClientLJP Create();

        ILJPMessageFactory CreateMessageFactory(string version);
    }
}
