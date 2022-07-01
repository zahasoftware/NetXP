
using NetXP.DependencyInjection;
using NetXP.Factories;

namespace NetXP.Cryptography.Implementations
{
    public class AsymetricFactory : INameResolverFactory<IAsymetricCrypt>
    {
        public AsymetricFactory(IContainer uc)
        {
            this.uc = uc;
        }

        private readonly IContainer uc;

        public IAsymetricCrypt Resolve(string name = "")
        {
            return uc.Resolve<IAsymetricCrypt>();
        }
    }
}