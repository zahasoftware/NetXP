
using NetXP.NetStandard.DependencyInjection;
using NetXP.NetStandard.Factories;

namespace NetXP.NetStandard.Cryptography.Implementations
{
    public class AsymetricFactory : INameResolverFactory<IAsymetricCrypt>
    {
        public AsymetricFactory(IContainer uc)
        {
            this.uc = uc;
        }

        private IContainer uc;

        public IAsymetricCrypt Resolve(string name = "")
        {
            return uc.Resolve<IAsymetricCrypt>();
        }
    }
}