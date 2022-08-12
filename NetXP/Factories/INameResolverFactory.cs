using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.Factories
{
    public interface INameResolverFactory<T> where T : class
    {
        T Resolve(string name = null);
    }
}
