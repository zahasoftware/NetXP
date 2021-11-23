using Lamar.IoC.Instances;
using System;
using System.Collections.Generic;
using static Lamar.ServiceRegistry;

namespace NetXP.NetStandard.DependencyInjection.Implementations.LamarDI
{
    public class LamarSelectorExpression<TImpl, TInter> : ICtorSelectorExpression<TImpl, TInter>
        where TInter : class
        where TImpl : class, TInter
    {
        private InstanceExpression<TInter> @for;
        private ConstructorInstance<TImpl, TInter> use;
        private Action<DILifeTime, ConstructorInstance<TImpl, TInter>> setLifeTime;
        private DILifeTime lifetime;

        public string instanceName { get; private set; }

        public void Empty()
        {
            if (instanceName != null)
            {
                use.Name = instanceName;
            }

            this.setLifeTime(this.lifetime, use);
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
            InstanceExpression<TInter> @for,
            ConstructorInstance<TImpl, TInter> use,
            string instanceName,
            DILifeTime lifetime,
            Action<DILifeTime, ConstructorInstance<TImpl, TInter>> setLifeTime)
        {
            this.@for = @for;
            this.use = use;
            this.instanceName = instanceName;
            this.lifetime = lifetime;
            this.setLifeTime = setLifeTime;
        }


    }
}