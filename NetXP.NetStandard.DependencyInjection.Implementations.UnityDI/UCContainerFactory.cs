using System;
using System.Collections.Generic;
using System.Text;

namespace NetXP.NetStandard.DependencyInjection.Implementations.UnityDI
{
    public class UCContainerFactory : IContainerFactory
    {
        public IContainer Create()
        {
            return new UContainer(new Unity.UnityContainer());
        }
    }
}
