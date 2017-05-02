using System;

namespace NetXP.NetStandard.DependencyInjection
{
    public interface IConfiguration
    {
        void Configure(Action<IRegister> register);
    }
}