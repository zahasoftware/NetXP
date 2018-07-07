namespace NetXP.NetStandard.Network.TCP
{
    public interface IServerConnectorFactory
    {
        IServerConnector Create(ConnectorFactory networkFactory);
        IServerConnector Create();
    }
}