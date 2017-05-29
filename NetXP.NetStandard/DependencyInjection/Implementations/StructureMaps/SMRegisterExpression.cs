using NetXP.NetStandard.DependencyInjection;
using StructureMap;
using StructureMap.Pipeline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.NetStandard.DependencyInjection.Implementations.StructureMaps
{
    public class SMRegisterExpression : IRegister
    {
        private ConfigurationExpression container;

        public SMRegisterExpression(ConfigurationExpression container)
        {
            this.container = container;
        }

        public void Register<TInterface, TImplement>() where TImplement : TInterface
        {
            this.container.For<TInterface>().Use<TImplement>();
        }

        public void Register<TInterface, TImplement>(LifeTime lifeTime) where TImplement : TInterface
        {
            var register = this.container.For<TInterface>().Use<TImplement>();
            SetLifeTime(lifeTime, register);
        }

        public void Register<TInterface, TImplement>(string name) where TImplement : TInterface
        {
            this.container.For<TInterface>().Use<TImplement>().Named(name);
        }

        public void Register<TInterface, TImplement>(string name, LifeTime lifeTime) where TImplement : TInterface
        {
            var register = this.container.For<TInterface>().Use<TImplement>().Named(name);
            SetLifeTime(lifeTime, register);
        }



        public void Register<TInterface, TImplement>(LifeTime lifeTime,
                                                     Action<ICtorSelectorExpression<TImplement, TInterface>> ctorInjectorExpression)
                                                     where TImplement : TInterface
        {
            var register = this.container.For<TInterface>()
            .Use<TImplement>()
            .Transient();

            var ctorSelectorExpression = new SMCtorSelectorExpression<TImplement, TInterface>();
            ctorSelectorExpression.Register(register);
            ctorInjectorExpression(ctorSelectorExpression);

            SetLifeTime(lifeTime, register);
        }

        public void Register<TInterface, TImplement>(string name,
                                                     LifeTime lifeTime,
                                                     Action<ICtorSelectorExpression<TImplement, TInterface>> ctorInjectorExpression)
                                                     where TImplement : TInterface
        {
            var register = this.container.For<TInterface>()
            .Use<TImplement>()
            .Named(name)
            .Transient();

            var ctorSelectorExpression = new SMCtorSelectorExpression<TImplement, TInterface>();
            ctorSelectorExpression.Register(register);
            ctorInjectorExpression(ctorSelectorExpression);

            SetLifeTime(lifeTime, register);
        }

        public void RegisterInstance<TInterface>(TInterface instance, LifeTime lifeTime) 
        {
            var register = this.container.For<TInterface>().Use(o => instance);
            SetLifeTime(lifeTime, register);
        }

        public void RegisterInstance<TInterface>(string name, TInterface instance, LifeTime lifeTime) 
        {
            var register = this.container.For<TInterface>().Use(o => instance).Named(name);
            SetLifeTime(lifeTime, register);
        }

        private static void SetLifeTime<TInterface, TImplement>(LifeTime lifeTime, StructureMap.Pipeline.SmartInstance<TImplement, TInterface> register) where TImplement : TInterface
        {
            if (lifeTime == LifeTime.Scoped)
            {
                register.Transient();
            }
            else if (lifeTime == LifeTime.Singleton)
            {
                register.Singleton();
            }
            else if (lifeTime == LifeTime.Trasient)
            {
                register.AlwaysUnique();
            }
        }

        private static void SetLifeTime<TInterface, TImplement>(LifeTime lifeTime, StructureMap.Pipeline.LambdaInstance<TImplement, TInterface> register) where TImplement : TInterface
        {
            if (lifeTime == LifeTime.Scoped)
            {
                register.Transient();
            }
            else if (lifeTime == LifeTime.Singleton)
            {
                register.Singleton();
            }
            else if (lifeTime == LifeTime.Trasient)
            {
                register.AlwaysUnique();
            }
        }
    }
}
