using System;

namespace NetXP.NetStandard.DependencyInjection
{
    public interface ICtorInjectorExpression
    {
        void InjectInstance(string parameterName, object instance);
    }
}