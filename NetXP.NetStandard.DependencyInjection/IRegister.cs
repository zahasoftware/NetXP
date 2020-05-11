using System;

namespace NetXP.NetStandard.DependencyInjection
{
    public interface IRegister
    {
        void Register<TInterface, TImplement>() where TImplement : TInterface;
        void Register<TInterface, TImplement>(DILifeTime lifeTime) where TImplement : TInterface;

        void Register<TInterface, TImplement>(string name) where TImplement : TInterface;
        void Register<TInterface, TImplement>(string name, DILifeTime lifeTime) where TImplement : TInterface;

        void RegisterInstance<TInterface>(TInterface instance, DILifeTime lifeTime);
        void RegisterInstance<TInterface>(string name, TInterface instance, DILifeTime lifeTime);

        void Register<TInterface, TImplement>(DILifeTime lifeTime,
                                              Action<ICtorSelectorExpression<TImplement, TInterface>> ctorInjectorExpression)
                                              where TImplement : TInterface;
        void Register<TInterface, TImplement>(string name, DILifeTime lifeTime,
                                              Action<ICtorSelectorExpression<TImplement, TInterface>> ctorInjectorExpression)
                                              where TImplement : TInterface;
    }
}