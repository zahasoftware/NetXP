using NetXP.NetStandard.Installers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.NetStandard.Installers
{
    public interface IFactoryStep
    {
        List<IStep> ResolveAll();
    }
}
