using System;
using System.Collections.Generic;
using System.Text;

namespace NetXP.NetStandard.DependencyInjection.Implementations.UnityDI
{
    public class UCtorInjectorExpression : ICtorInjectorExpression
    {
        private List<UCtorInjector> ctorInjectors = new List<UCtorInjector>();
        internal List<UCtorInjector> CtorInjectors { get => ctorInjectors; set => ctorInjectors = value; }

        public void InjectInstance(string parameterName, object instance)
        {
            CtorInjectors.Add(new UCtorInjector { ParameterName = parameterName, Intance = instance });
        }
    }
}
