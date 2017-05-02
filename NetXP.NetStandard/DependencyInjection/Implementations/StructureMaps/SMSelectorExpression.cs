using System;
using System.Collections.Generic;
using StructureMap.Pipeline;

namespace NetXP.NetStandard.DependencyInjection.Implementations.StructureMaps
{
    public class SMCtorSelectorExpression<TImpl, TInter> : ICtorSelectorExpression<TImpl, TInter> where TImpl : TInter
    {
        private SmartInstance<TImpl, TInter> register;

        public void InjectInstance<T>(T instance)
        {
            register.Ctor<T>().Is(o => instance);
        }

        public void WithParameter<T>()
        {
            register.Ctor<T>();
        }

        public void WithParameter<T>(string name)
        {
            register.Ctor<T>(name);
        }

        internal void Register(SmartInstance<TImpl, TInter> register)
        {
            this.register = register;
        }
    }
}