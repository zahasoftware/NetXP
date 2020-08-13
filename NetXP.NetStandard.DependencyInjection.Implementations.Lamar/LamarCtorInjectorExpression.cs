using System;
using System.Collections.Generic;
using System.Text;

namespace NetXP.NetStandard.DependencyInjection.Implementations.LamarDI
{
    public class LamarCtorInjectorExpression : ICtorInjectorExpression
    {
        private List<LamarCtorInjector> ctorInjectors = new List<LamarCtorInjector>();
        internal List<LamarCtorInjector> CtorInjectors { get => ctorInjectors; set => ctorInjectors = value; }

        public void InjectInstance(string parameterName, object instance)
        {
            CtorInjectors.Add(new LamarCtorInjector { ParameterName = parameterName, Intance = instance });
        }
    }
}
