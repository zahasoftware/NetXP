using System.Net;

namespace NetXP.NetStandard.Configuration
{
    /// <summary>
    /// Class for secure config, that use remote keys and salt to encrypt an and decrypt
    /// </summary>
    public interface ISecureRemoteSDMConf<T> where T : class
    {
        T Read(string identifier, IPEndPoint remoteServer);

        void Save(string uniqueCode, T toSerialize, IPEndPoint remoteServer);
    }
}
