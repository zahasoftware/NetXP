using System;

namespace NetXP.DependencyInjection.Implementations.UnityDI
{
    public class UCtorSelector
    {
        private Type type;
        private object instance;

        public UCtorSelector(Type type)
        {
            this.Type = type;
        }

        public UCtorSelector(object instance)
        {
            this.Instance = instance;
        }

        public Type Type { get => type; set => type = value; }
        public object Instance { get => instance; set => instance = value; }
    }
}