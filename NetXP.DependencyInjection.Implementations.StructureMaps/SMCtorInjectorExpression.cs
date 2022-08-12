using StructureMap;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetXP.DependencyInjection.Implementations.StructureMaps
{
    public class SMCtorInjectorExpression : ICtorInjectorExpression
    {
        private List<SMCtorInjector> ctorInjectors = new List<SMCtorInjector>();
        internal List<SMCtorInjector> CtorInjectors { get => ctorInjectors; set => ctorInjectors = value; }

        public void InjectInstance(string parameterName, object instance)
        {
            CtorInjectors.Add(new SMCtorInjector { ParameterName = parameterName, Intance = instance });
        }
    }
}
