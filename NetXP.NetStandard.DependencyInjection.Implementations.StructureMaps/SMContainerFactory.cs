using System;
using System.Collections.Generic;
using System.Text;

namespace NetXP.DependencyInjection.Implementations.StructureMaps
{
    public class SMContainerFactory : IContainerFactory
    {
        public IContainer Create()
        {
            return new SMContainer(new StructureMap.Container());
        }
    }
}
