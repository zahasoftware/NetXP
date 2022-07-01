using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.Network.LittleJsonProtocol
{
    public class LJPResponse<T> : LJPHeader
    {
        public T oObject { get; set; }
    }
}
