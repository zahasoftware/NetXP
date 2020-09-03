using System;

namespace NetXP.NetStandard.DependencyInjection.Implementations.LamarDI
{
    public class LamarCtorSelector
    {
        private Type type;
        private object instance;

        public LamarCtorSelector(Type type)
        {
            this.Type = type;
        }

        public LamarCtorSelector(object instance)
        {
            this.Instance = instance;
        }

        public Type Type { get => type; set => type = value; }
        public object Instance { get => instance; set => instance = value; }
    }
}