using NetXP.NetStandard.DependencyInjection;
using NetXP.NetStandard.Factories;
using System;
using System.Collections.Generic;
using System.Text;
using Unity;

namespace NetXP.NetStandard.Cryptography.Implementations
{
    class CryptographyCompositionRoot
    {

        public void RegisterCryptography(IRegister uc)
        {
            uc.Register<INameResolverFactory<IAsymetricCrypt>, AsymetricFactory>(DILifeTime.Singleton);
            uc.Register<IHash, HashMD5>(HashType.MD5.ToString(), DILifeTime.Trasient);
            uc.Register<IHash, HashSHA256>(DILifeTime.Trasient);
            uc.Register<IHashFactory, HashFactory>(DILifeTime.Singleton);
        }
    }
}
