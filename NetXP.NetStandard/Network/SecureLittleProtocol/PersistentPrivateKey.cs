using NetXP.NetStandard.Cryptography;
using System;

namespace NetXP.NetStandard.Network
{
    public class PersistentPrivateKey
    {
        internal string File;
        private int port;

        public PrivateKey PrivateKey { get; set; }
        public string Server { get; set; }
        public DateTime ExpirationDate { get; set; }
        public PublicKey PublicKeyRemote { get; set; }
        public bool FromServer { get; set; }

        public int Port
        {
            get => port;
            set
            {
                if (!ushort.TryParse(port.ToString(), out ushort x))
                    throw new ArgumentException("Invalid Port");
                port = value;
            }
        }
    }
}