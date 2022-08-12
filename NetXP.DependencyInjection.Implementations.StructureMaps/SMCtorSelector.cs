using System;

namespace NetXP.DependencyInjection.Implementations.StructureMaps
{
    public class SMCtorSelector
    {
        private Type type;
        private object instance;

        public SMCtorSelector(Type type)
        {
            this.Type = type;
        }

        public SMCtorSelector(object instance)
        {
            this.Instance = instance;
        }

        public Type Type { get => type; set => type = value; }
        public object Instance { get => instance; set => instance = value; }
    }
}