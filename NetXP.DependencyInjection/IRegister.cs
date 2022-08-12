using System;

namespace NetXP.DependencyInjection
{
    public interface IRegister
    {
        void Register<TInterface, TImplement>()
            where TInterface : class
            where TImplement : class, TInterface;
        void Register<TInterface, TImplement>(DILifeTime lifeTime)
            where TInterface : class
            where TImplement : class, TInterface;

        void Register<TInterface, TImplement>(string name)
            where TInterface : class
            where TImplement : class, TInterface;
        void Register<TInterface, TImplement>(string name, DILifeTime lifeTime)
            where TInterface : class
            where TImplement : class, TInterface;

        void RegisterInstance<TInterface>(TInterface instance) where TInterface : class;
        void RegisterInstance<TInterface>(string name, TInterface instance) where TInterface : class;

        void Register<TInterface, TImplement>(DILifeTime lifeTime,
                                              Action<ICtorSelectorExpression<TImplement, TInterface>> ctorInjectorExpression)
            where TInterface : class
            where TImplement : class, TInterface;
        void Register<TInterface, TImplement>(string name, DILifeTime lifeTime,
                                              Action<ICtorSelectorExpression<TImplement, TInterface>> ctorInjectorExpression)
            where TInterface : class
            where TImplement : class, TInterface;
    }
}