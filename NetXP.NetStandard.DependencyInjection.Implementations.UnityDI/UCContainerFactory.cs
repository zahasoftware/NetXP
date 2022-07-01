using System;
using System.Collections.Generic;
using System.Text;

namespace NetXP.DependencyInjection.Implementations.UnityDI
{
    public class UCContainerFactory : IContainerFactory
    {
        public IContainer Create()
        {
            return new UContainer(new Unity.UnityContainer());
        }
    }
}
