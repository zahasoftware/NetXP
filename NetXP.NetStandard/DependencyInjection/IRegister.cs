using System;

namespace NetXP.NetStandard.DependencyInjection
{
    public interface IRegister
    {
        void Register<TInterface, TImplement>() where TImplement : TInterface;
        void Register<TInterface, TImplement>(LifeTime lifeTime) where TImplement : TInterface;

        void Register<TInterface, TImplement>(string name) where TImplement : TInterface;
        void Register<TInterface, TImplement>(string name, LifeTime lifeTime) where TImplement : TInterface;

        void RegisterInstance<TInterface, TImplement>(TInterface instance, LifeTime lifeTime) where TImplement : TInterface;
        void RegisterInstance<TInterface, TImplement>(string name, TInterface instance, LifeTime lifeTime) where TImplement : TInterface;

        void Register<TInterface, TImplement>(LifeTime lifeTime,
                                              Action<ICtorSelectorExpression<TImplement, TInterface>> ctorInjectorExpression)
                                              where TImplement : TInterface;
        void Register<TInterface, TImplement>(string name, LifeTime lifeTime,
                                              Action<ICtorSelectorExpression<TImplement, TInterface>> ctorInjectorExpression)
                                              where TImplement : TInterface;
    }
}