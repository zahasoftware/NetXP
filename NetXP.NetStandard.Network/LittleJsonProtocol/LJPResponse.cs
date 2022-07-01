using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.Network.LittleJsonProtocol
{
    public class LJPResponse : LJPHeader
    {
        public Type tpeObject;
        public dynamic oObject;
    }
}
