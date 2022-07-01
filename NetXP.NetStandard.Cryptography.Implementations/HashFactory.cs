using NetXP.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.Cryptography.Implementations
{
    public class HashFactory : IHashFactory
    {
        private readonly IContainer container;

        public HashFactory(IContainer unityContainer)
        {
            this.container = unityContainer;

        }
        public IHash Create(HashType hashType)
        {
            return hashType == HashType.SHA256 ? this.container.Resolve<IHash>()
                                               : this.container.Resolve<IHash>($"{hashType}");
        }
    }
}
