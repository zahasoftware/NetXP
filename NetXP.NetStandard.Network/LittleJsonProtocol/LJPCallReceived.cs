using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using NetXP.NetStandard.Network.LittleJsonProtocol.Implementations;

namespace NetXP.NetStandard.Network.LittleJsonProtocol
{
    /// <summary>
    /// Used by LJPClient.ReceiveCall method.
    /// </summary>
    public class LJPCallReceived : LJPCallHeader
    {
        public Type Interface { get; internal set; }
        public MethodInfo Method { get; internal set; }

        public override string ToString()
        {
            return $"{Interface?.Name}.{Method?.Name} {{{string.Join("||", this.Parameters)}}} ";
        }
    }
}
