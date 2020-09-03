using System;
using System.Collections.Generic;

namespace NetXP.NetStandard.DependencyInjection
{
    public interface IResolver
    {
        TInterface Resolve<TInterface>();
        TInterface Resolve<TInterface>(string name);
        IEnumerable<TInterface> ResolveAll<TInterface>();
        object Resolve(Type interfaceType);
    }
}