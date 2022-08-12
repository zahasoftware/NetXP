using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.Cryptography
{
    public class PublicKey
    {
        public byte[] yModulus { get; set; }

        public byte[] yExponent { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            var pk = obj as PublicKey;
            return
                pk.yExponent != null
                && pk.yModulus != null
                && yExponent != null
                && yModulus != null
                && pk.yExponent.SequenceEqual(yExponent)
                && pk.yModulus.SequenceEqual(yModulus);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return BitHelper.ToInt32(yExponent, 0);
        }
    }
}
