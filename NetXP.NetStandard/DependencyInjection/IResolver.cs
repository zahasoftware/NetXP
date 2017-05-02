using System;

namespace NetXP.NetStandard.DependencyInjection
{
    public interface IResolver
    {
        TInterface Resolve<TInterface>();
        TInterface Resolve<TInterface>(string name);
        TInterface Resolve<TInterface>(Action<ICtorInjectorExpression> ctorInjectorExpression);
        TInterface Resolve<TInterface>(string name, Action<ICtorInjectorExpression> ctorInjectorExpression);

        object Resolve(Type interfaceType);
    }
}