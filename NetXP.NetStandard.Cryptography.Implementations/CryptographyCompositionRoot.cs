using NetXP.NetStandard.DependencyInjection;
using NetXP.NetStandard.Factories;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetXP.NetStandard.Cryptography.Implementations
{
    public static class CryptographyCompositionRoot
    {

        public static void RegisterCryptography(this IRegister uc)
        {
            uc.Register<INameResolverFactory<IAsymetricCrypt>, AsymetricFactory>(DILifeTime.Singleton);
            uc.Register<IHash, HashMD5>(HashType.MD5.ToString(), DILifeTime.Trasient);
            uc.Register<IHash, HashSHA256>(DILifeTime.Trasient);
            uc.Register<IHashFactory, HashFactory>(DILifeTime.Singleton);
            uc.Register<ISymetricCrypt, SymetricAes>();
            uc.Register<IAsymetricCrypt, AsymetricCryptWithMSRSA>();
        }
    }
}
