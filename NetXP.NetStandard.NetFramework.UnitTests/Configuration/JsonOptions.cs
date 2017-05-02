using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetXP.NetStandard.Serialization;
using System.IO;

namespace NetXP.NetStandard.NetFramework.UnitTests.Configuration
{
    public class JsonOptions<T> : IOptions<T> where T : class, new()
    {
        public T Value => GetValue();
        private T GetValue()
        {
            throw new NotImplementedException();
        }
    }
}
