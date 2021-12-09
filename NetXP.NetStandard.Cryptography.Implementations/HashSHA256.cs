using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.NetStandard.Cryptography.Implementations
{
    public class HashSHA256 : HashBase
    {
        public HashSHA256():base(System.Security.Cryptography.SHA256.Create())
        {
        }
    }
}
