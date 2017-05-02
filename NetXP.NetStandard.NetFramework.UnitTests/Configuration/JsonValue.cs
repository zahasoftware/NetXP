using System;

namespace NetXP.NetStandard.NetFramework.UnitTests.Configuration
{
    public class JsonValue<T>
    {
        public Type Type { internal get; set; }
        public T Value
        {
            get => value;
            set
            {
                Type = value.GetType();
                this.value = value;
            }
        }

        private T value;
    }
}