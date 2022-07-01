using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.Cryptography.Implementations
{
    public class HashMD5 : HashBase
    {
        public HashMD5() : base(System.Security.Cryptography.MD5.Create())
        {
        }
    }
}
