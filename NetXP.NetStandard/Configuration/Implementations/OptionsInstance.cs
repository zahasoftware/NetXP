using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetXP.Configuration.Implementations
{
    public class OptionsInstance<T> : IOptions<T> where T : class, new()
    {
        private readonly T instance;

        public OptionsInstance(T instance)
        {
            this.instance = instance;
        }
        public T Value => instance;
    }
}
