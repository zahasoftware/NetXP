using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.Network.TCP
{
    public interface IClientConnectorFactory
    {
        IClientConnector Create( params object[] @params);
    }
}