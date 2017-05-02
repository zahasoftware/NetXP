using NetXP.NetStandard.Network.i;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.NetStandard.Network.LittleJsonProtocol
{
    public interface IFactoryClientLJP
    {
        IClientLJP Create();

        ILJPMessageFactory CreateMessageFactory(string version);
    }
}
