using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.NetStandard.Cryptography
{
    public class SymetricKey
    {
        public SymetricKey()
        {
            this.iIteration = 1042;
        }

        private byte[] _yKey;
        public byte[] yKey
        {
            get
            {
                return _yKey;
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

                    _yKey = yBytes16;
                }
                else if (value.Length > 16) //Cut to get 16 bytes
                {
                    _yKey = value.Take(16).ToArray();
                }
                else
                {
                    _yKey = value;
                }

                if (ySalt == null || ySalt.Length == 0)
                {
                    ySalt = yKey;
                }
            }
        }

        private byte[] _ySalt;
        public byte[] ySalt
        {
            get { return _ySalt; }
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

                    _ySalt = yBytes16;
                }
                else if (value.Length > 16) //Cut to get 16 bytes
                {
                    _ySalt = value.Take(16).ToArray();
                }
                else
                {
                    _ySalt = value;
                }
            }
        }

        public int iIteration;
    }
}
