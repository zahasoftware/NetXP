using System;

namespace NetXP.NetStandard.DependencyInjection
{
    public interface ICtorSelectorExpression<TImplement, TInterface> where TImplement : TInterface
    {
        void WithParameter<T>();
        void InjectInstance<T>(T instance);
        void Empty();
    }
}