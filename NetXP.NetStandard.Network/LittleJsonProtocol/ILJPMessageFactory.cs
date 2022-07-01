namespace NetXP.Network.LittleJsonProtocol
{
    public interface ILJPMessageFactory
    {
        string Parse(LJPCallReceived oLJPCallResponse, string sObject);
        string Parse(LJPCall oLJPCallResponse);
    }
}