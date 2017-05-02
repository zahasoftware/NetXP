using System;
using System.Collections.Generic;
using System.Text;

namespace NetXP.NetStandard.Network.TCP
{
    public interface IFactoryProducer
    {
        IFactoryClientTCP Create(NetworkFactory networkFactory);
        IFactoryServer CreateServer(NetworkFactory networkFactory);
    }
}
