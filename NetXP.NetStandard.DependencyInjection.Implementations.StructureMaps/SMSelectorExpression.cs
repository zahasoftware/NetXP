using System;
using System.Collections.Generic;
using StructureMap.Pipeline;
using StructureMap.Configuration.DSL.Expressions;

namespace NetXP.DependencyInjection.Implementations.StructureMaps
{
    public class SMCtorSelectorExpression<TImpl, TInter> : ICtorSelectorExpression<TImpl, TInter>
        where TInter : class
        where TImpl : class, TInter
    {
        private SmartInstance<TImpl, TInter> use;
        private CreatePluginFamilyExpression<TInter> @for;
        private Action<DILifeTime, LambdaInstance<TImpl, TInter>> setLifeTime;
        private DILifeTime lifetime;

        public string instanceName { get; private set; }

        public void Empty()
        {
            var use = @for.Use(() => Activator.CreateInstance<TImpl>());
            if (instanceName != null)
            {
                use.Named(instanceName);
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
            CreatePluginFamilyExpression<TInter> @for,
            SmartInstance<TImpl, TInter> use,
            string instanceName,
            DILifeTime lifetime,
            Action<DILifeTime, LambdaInstance<TImpl, TInter>> setLifeTime)
        {
            this.@for = @for;
            this.use = use;
            this.instanceName = instanceName;
            this.lifetime = lifetime;
            this.setLifeTime = setLifeTime;
        }


    }
}