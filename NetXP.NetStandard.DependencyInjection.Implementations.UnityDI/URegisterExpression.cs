using NetXP.NetStandard.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;
using Unity.Lifetime;

namespace NetXP.NetStandard.DependencyInjection.Implementations.UnityDI
{
    public class URegisterExpression : IRegister
    {
        private readonly IUnityContainer container;

        public URegisterExpression(IUnityContainer container)
        {
            this.container = container;
        }

        public void Register<TInterface, TImplement>()
            where TInterface : class
            where TImplement : class, TInterface
        {
            this.container.RegisterType<TInterface, TImplement>();
        }

        public void Register<TInterface, TImplement>(DILifeTime lifeTime)
            where TInterface : class
            where TImplement : class, TInterface
        {
            switch (lifeTime)
            {
                case DILifeTime.Singleton:
                    this.container.RegisterType<TInterface, TImplement>(new Unity.Lifetime.ContainerControlledLifetimeManager());
                    break;
                case DILifeTime.Trasient:
                    this.container.RegisterType<TInterface, TImplement>(new Unity.Lifetime.PerResolveLifetimeManager());
                    break;
                case DILifeTime.Scoped:
                    this.container.RegisterType<TInterface, TImplement>(new Unity.Lifetime.PerThreadLifetimeManager());
                    break;
            }
        }

        public void Register<TInterface, TImplement>(string name)
            where TInterface : class
            where TImplement : class, TInterface
        {
            this.container.RegisterType<TInterface, TImplement>(name);
        }

        public void Register<TInterface, TImplement>(string name, DILifeTime lifeTime)
            where TInterface : class
            where TImplement : class, TInterface
        {
            this.container.RegisterType<TInterface, TImplement>(name);
            switch (lifeTime)
            {
                case DILifeTime.Singleton:
                    this.container.RegisterType<TInterface, TImplement>(name, new ContainerControlledLifetimeManager());
                    break;
                case DILifeTime.Trasient:
                    this.container.RegisterType<TInterface, TImplement>(name, new PerResolveLifetimeManager());
                    break;
                case DILifeTime.Scoped:
                    this.container.RegisterType<TInterface, TImplement>(name, new PerThreadLifetimeManager());
                    break;
            }
        }



        public void Register<TInterface, TImplement>(DILifeTime lifeTime,
                                                     Action<ICtorSelectorExpression<TImplement, TInterface>> ctorInjectorExpression)
            where TInterface : class
            where TImplement : class, TInterface
        {
            //this.container.RegisterType<TInterface>();

            //var ctorSelectorExpression = new UCtorSelectorExpression<TImplement, TInterface>();
            //ctorSelectorExpression.Register(@for, use, null, lifeTime, SetLifeTime);//Use, lifeTime, setLifeTime and Name only used for Empty constructors

            //ctorInjectorExpression(ctorSelectorExpression);

            //SetLifeTime(lifeTime, use);
        }

        public void Register<TInterface, TImplement>(string name,
                                                     DILifeTime lifeTime,
                                                     Action<ICtorSelectorExpression<TImplement, TInterface>> ctorInjectorExpression)
            where TInterface : class
            where TImplement : class, TInterface
        {
            //    var register = this.configuration.For<TInterface>();
            //    var use = register.Use<TImplement>();
            //    use.Named(name);

            //    var ctorSelectorExpression = new UCtorSelectorExpression<TImplement, TInterface>();
            //    ctorSelectorExpression.Register(register, use, name, lifeTime, SetLifeTime);//Use, lifeTime, setLifeTime and Name only used for Empty constructors
            //    ctorInjectorExpression(ctorSelectorExpression);

            //    register.Use(() => Activator.CreateInstance<TImplement>());

            //    SetLifeTime(lifeTime, use);
        }

        public void RegisterInstance<TInterface>(TInterface instance)
          where TInterface : class
        {
            this.container.RegisterInstance(instance);
        }

        public void RegisterInstance<TInterface>(TInterface instance, DILifeTime lifeTime)
            where TInterface : class
        {
            switch (lifeTime)
            {
                case DILifeTime.Singleton:
                    this.container.RegisterInstance(instance, new ContainerControlledLifetimeManager());
                    break;
                case DILifeTime.Trasient:
                    this.container.RegisterInstance(instance, new PerResolveLifetimeManager());
                    break;
                case DILifeTime.Scoped:
                    this.container.RegisterInstance(instance, new PerResolveLifetimeManager());
                    break;
            }
        }

        public void RegisterInstance<TInterface>(string name, TInterface instance, DILifeTime lifeTime)
        {
            switch (lifeTime)
            {
                case DILifeTime.Singleton:
                    this.container.RegisterInstance(name, instance, new ContainerControlledLifetimeManager());
                    break;
                case DILifeTime.Trasient:
                    this.container.RegisterInstance(name, instance, new PerResolveLifetimeManager());
                    break;
                case DILifeTime.Scoped:
                    this.container.RegisterInstance(name, instance, new PerResolveLifetimeManager());
                    break;
            }
        }


    }
}
