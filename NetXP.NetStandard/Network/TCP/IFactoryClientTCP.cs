using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.NetStandard.Network.TCP
{
    public interface IFactoryClientTCP
    {
        ITCPClient Create(params object[] aParams);
    }
}
