using System;

namespace NetXP.DependencyInjection
{
    public interface ICtorInjectorExpression
    {
        void InjectInstance(string parameterName, object instance);
    }
}