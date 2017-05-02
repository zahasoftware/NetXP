using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.NetStandard.Reflection
{
    public interface IReflector
    {
        //IList<Type> aNameSpaces { get; }
        object InvokeMethod(Type tpeNamespace, string sInterface, string sMethod, object[] aParams);
        MethodInfo ReflectMethod(Type tpeNamespace, string sInterface, string sMethod);
        object InvokeMethodWithJSONParameters(Type tpeNamespace, string sInterface, string sMethod, string[] aParams);
        Type GetType(Type tpeNamespace, string sType);
        bool TryGetType(Type tpeNamespace, string sType, out Type tpeInterface);

    }
}
