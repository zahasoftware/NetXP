using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Unity;

namespace NetXP.NetStandard.DependencyInjection.Implementations.UnityDI
{
    public class UCtorSelectorExpression<TImpl, TInter> : ICtorSelectorExpression<TImpl, TInter>
        where TInter : class
        where TImpl : class, TInter
    {

        public UCtorSelectorExpression(IUnityContainer uc, List<object> parameters)
        {
            this.uc = uc;
            this.parameters = parameters;
        }

        private IUnityContainer uc;
        private readonly List<object> parameters;

        public void Empty()
        {
            this.parameters.Clear();
        }

        public void InjectInstance<T>(T instance)
        {
            this.parameters.Add(instance);
        }

        public void WithParameter<T>()
        {
            var instance = this.uc.Resolve<T>();
            this.parameters.Add(instance);
        }

        public void WithParameter<T>(string name)
        {
            var instance = this.uc.Resolve<T>(name);
            this.parameters.Add(instance);
        }




    }
}