using NetXP.Cryptography;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.Network
{
    public interface IPersistentPrivateKeyProvider
    {
        /// <summary>
        /// Get the persistent private key from some store.
        /// </summary>
        /// <param name="sServer">domain or ip</param>
        /// <param name="iPort">port where to connect</param>
        /// <returns>Private key of sServer and iPort</returns>
        PersistentPrivateKey Read(string sServer, int iPort);

        /// <summary>
        /// Get the persistent private key from publick key.
        /// </summary>
        /// <param name="PublicKey">public key</param>
        /// <returns>Persistent private key</returns>
        PersistentPrivateKey Read(PublicKey PublicKey);

        /// <summary>
        /// Save the persistent private key.
        /// </summary>
        /// <param name="PersistentPrivateKey"></param>
        void Save(PersistentPrivateKey PersistentPrivateKey);
    }
}
