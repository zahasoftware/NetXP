using Lamar.IoC.Instances;
using System;
using System.Collections.Generic;
using static Lamar.ServiceRegistry;

namespace NetXP.DependencyInjection.Implementations.LamarDI
{
    public class LamarSelectorExpression<TImpl, TInter> : ICtorSelectorExpression<TImpl, TInter>
        where TInter : class
        where TImpl : class, TInter
    {
        private ConstructorInstance<TImpl, TInter> use;

        public void Empty()
        {
        }

        public void InjectInstance<T>(T instance)
        {
            use.Ctor<T>().Is(o => instance);
        }

        public void WithParameter<T>()
        {
            use.Ctor<T>();
        }

        public void WithParameter<T>(string name)
        {
            use.Ctor<T>(name);
        }

        internal void Register(
            ConstructorInstance<TImpl, TInter> use
            )
        {
            this.use = use;
        }


    }
}