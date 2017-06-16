using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.NetStandard.Cryptography
{
    [DataContract]
    public class SymetricKey
    {
        public SymetricKey()
        {
            this.Iteration = 1042;
        }

        [DataMember(Name = "k")]
        private byte[] key;
        public byte[] Key
        {
            get
            {
                return key;
            }
            set
            {
                if (value.Length < 16) //Padding with X to get 16 bytes
                {
                    byte[] yBytes16 = new byte[16];
                    int i = 0;
                    for (; i < value.Length; i++)
                    {
                        yBytes16[i] = value[i];
                    }

                    for (; i < 16; i++)
                    {
                        yBytes16[i] = (byte)'X';
                    }

                    key = yBytes16;
                }
                else if (value.Length > 16) //Cut to get 16 bytes
                {
                    key = value.Take(16).ToArray();
                }
                else
                {
                    key = value;
                }

                if (Salt == null || Salt.Length == 0)
                {
                    Salt = Key;
                }
            }
        }

        [DataMember(Name = "s")]
        private byte[] salt;
        public byte[] Salt
        {
            get { return salt; }
            set
            {
                if (value.Length < 16) //Padding with X to get 16 bytes
                {
                    byte[] yBytes16 = new byte[16];
                    int i = 0;
                    for (; i < value.Length; i++)
                    {
                        yBytes16[i] = value[i];
                    }

                    for (; i < 16; i++)
                    {
                        yBytes16[i] = (byte)'X';
                    }

                    salt = yBytes16;
                }
                else if (value.Length > 16) //Cut to get 16 bytes
                {
                    salt = value.Take(16).ToArray();
                }
                else
                {
                    salt = value;
                }
            }
        }

        [DataMember(Name = "i")]
        public int Iteration;
    }
}
