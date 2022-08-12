using System;

namespace NetXP.DependencyInjection
{
    public interface IConfiguration
    {
        void Configure(Action<IRegister> register);
    }
}