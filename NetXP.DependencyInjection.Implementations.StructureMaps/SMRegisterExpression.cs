﻿using NetXP.DependencyInjection;
using StructureMap;
using StructureMap.Pipeline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.DependencyInjection.Implementations.StructureMaps
{
    public class SMRegisterExpression : IRegister
    {
        private ConfigurationExpression configuration;
        private readonly Container container;

        public SMRegisterExpression(ConfigurationExpression configuration, Container container)
        {
            this.configuration = configuration;
            this.container = container;
        }

        public void Register<TInterface, TImplement>()
            where TInterface : class
            where TImplement : class, TInterface
        {
            var register = this.configuration.For<TInterface>().Use<TImplement>();
            SetLifeTime(DILifeTime.Singleton, register);
        }

        public void Register<TInterface, TImplement>(DILifeTime lifeTime)
            where TInterface : class
            where TImplement : class, TInterface
        {
            var register = this.configuration.For<TInterface>().Use<TImplement>();
            SetLifeTime(lifeTime, register);
        }

        public void Register<TInterface, TImplement>(string name) where TInterface : class
            where TImplement : class, TInterface
        {
            this.configuration.For<TInterface>().Use<TImplement>().Named(name);
        }

        public void Register<TInterface, TImplement>(string name, DILifeTime lifeTime) where TInterface : class
            where TImplement : class, TInterface
        {
            var register = this.configuration.For<TInterface>().Use<TImplement>().Named(name);
            SetLifeTime(lifeTime, register);
        }



        public void Register<TInterface, TImplement>(DILifeTime lifeTime,
                                                     Action<ICtorSelectorExpression<TImplement, TInterface>> ctorInjectorExpression)
                                                      where TInterface : class
            where TImplement : class, TInterface
        {
            var @for = this.configuration.For<TInterface>();
            var use = @for.Use<TImplement>();

            var ctorSelectorExpression = new SMCtorSelectorExpression<TImplement, TInterface>();
            ctorSelectorExpression.Register(@for, use, null, lifeTime, SetLifeTime);//Use, lifeTime, setLifeTime and Name only used for Empty constructors
            ctorInjectorExpression(ctorSelectorExpression);

            SetLifeTime(lifeTime, use);
        }

        public void Register<TInterface, TImplement>(string name,
                                                     DILifeTime lifeTime,
                                                     Action<ICtorSelectorExpression<TImplement, TInterface>> ctorInjectorExpression)
            where TInterface : class
            where TImplement : class, TInterface
        {
            var register = this.configuration.For<TInterface>();
            var use = register.Use<TImplement>();
            use.Named(name);

            var ctorSelectorExpression = new SMCtorSelectorExpression<TImplement, TInterface>();
            ctorSelectorExpression.Register(register, use, name, lifeTime, SetLifeTime);//Use, lifeTime, setLifeTime and Name only used for Empty constructors
            ctorInjectorExpression(ctorSelectorExpression);

            register.Use(() => Activator.CreateInstance<TImplement>());

            SetLifeTime(lifeTime, use);
        }

        public void RegisterInstance<TInterface>(TInterface instance)
           where TInterface : class
        {
            var register = this.configuration.For<TInterface>().Use(o => instance);
        }

        public void RegisterInstance<TInterface>(string name, TInterface instance)
            where TInterface : class
        {
            this.configuration.For<TInterface>().Use(o => instance).Named(name);
        }

        private static void SetLifeTime<TInterface, TImplement>(DILifeTime lifeTime, StructureMap.Pipeline.SmartInstance<TImplement, TInterface> register)
            where TImplement : TInterface
        {
            if (lifeTime == DILifeTime.Scoped)
            {
                register.Transient();
            }
            else if (lifeTime == DILifeTime.Singleton)
            {
                register.Singleton();
            }
            else if (lifeTime == DILifeTime.Trasient)
            {
                register.AlwaysUnique();
            }
        }

        private static void SetLifeTime<TInterface, TImplement>(DILifeTime lifeTime, StructureMap.Pipeline.LambdaInstance<TImplement, TInterface> register)
            where TImplement : TInterface
        {
            if (lifeTime == DILifeTime.Scoped)
            {
                register.Transient();
            }
            else if (lifeTime == DILifeTime.Singleton)
            {
                register.Singleton();
            }
            else if (lifeTime == DILifeTime.Trasient)
            {
                register.AlwaysUnique();
            }
        }
    }
}
