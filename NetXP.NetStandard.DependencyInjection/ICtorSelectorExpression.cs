using System;

namespace NetXP.NetStandard.DependencyInjection
{
    public interface ICtorSelectorExpression<TImplement, TInterface>
        where TInterface : class
        where TImplement : class, TInterface
    {
        void WithParameter<T>();
        void InjectInstance<T>(T instance);
        void Empty();
    }
}