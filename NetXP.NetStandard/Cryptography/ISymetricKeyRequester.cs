using NetXP.NetStandard.Network.LittleJsonProtocol;

namespace NetXP.NetStandard.Cryptography
{
    /// <summary>
    /// This interface is for remote keys.
    /// </summary>
    public interface ISymetricKeyRequester
    {
        /// <summary>
        /// Get a key that was generate with NewKey 
        /// </summary>
        /// <returns></returns>
        SymetricKey GetKey(IClientLJP clientLJP);
        /// <summary>
        /// Generate a new key for encrypt or decrypt
        /// </summary>
        /// <returns></returns>
        SymetricKey NewKey(string uniqueRemoteCode, IClientLJP clientLJP);
    }
}
