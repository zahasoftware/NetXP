using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Runtime;
using NetXP.NetStandard.DependencyInjection;
using NetXP.NetStandard.Exceptions;

namespace NetXP.NetStandard.Reflection.Implementations
{
    public class Reflector : IReflector
    {
        private readonly IContainer container;

        public Reflector(IContainer container)
        {
            this.container = container;
        }

        public object InvokeMethod(Type tpeNamespace, string sInterface, string sMethod, object[] aParams)
        {
            object oInterfaceResolved;
            MethodInfo method;
            ResolveInterfaceAndMethod(tpeNamespace, sInterface, sMethod, out oInterfaceResolved, out method);

            //Serialicing Parameter
            ParameterInfo[] aParametersInfo = method.GetParameters();
            if (aParametersInfo.Count() != aParams.Length) throw new InvalidOperationException("Method don't have all parameters.");

            object oReturn = method.Invoke(oInterfaceResolved, aParams);
            return oReturn;
        }

        public object InvokeMethodWithJSONParameters(Type tpeNamespace, string sInterface, string sMethod, string[] aParams)
        {
            object oInterfaceResolved;
            MethodInfo method;
            this.ResolveInterfaceAndMethod(tpeNamespace, sInterface, sMethod, out oInterfaceResolved, out method);

            var aParameterInfo = method.GetParameters();

            List<object> aParametersToMethod = new List<object>();

            for (int i = 0; i < aParameterInfo.Count(); i++)// var oParameterInfoFE in aParameterInfo)
            {
                ParameterInfo oParameterInfo = aParameterInfo[i];
                var oJsonSerializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(oParameterInfo.ParameterType);
                using (MemoryStream oMS = new MemoryStream())
                {
                    using (StreamWriter oSW = new StreamWriter(oMS))
                    {
                        oSW.Write(aParams[i]);
                        oSW.Flush();
                        oMS.Position = 0;
                        var oObject = oJsonSerializer.ReadObject(oMS);
                        aParametersToMethod.Add(oObject);
                    }
                }
            }

            object oReturn = method.Invoke(oInterfaceResolved, aParametersToMethod.ToArray());
            return oReturn;
        }

        public Type GetType(Type referenceType, string typeToFound)
        {
            //Resolving Interface
            Type typeToTryResolve = null;
            typeToTryResolve = referenceType.GetTypeInfo().Assembly.GetTypes().SingleOrDefault(o => o.Name.Equals(typeToFound));

            if (typeToTryResolve == null)
            {
                throw new ArgumentException($"Type \"{typeToFound}\" not found in assembly with reference type \"{referenceType.Name}\"");
            }
            return typeToTryResolve;
        }

        public bool TryGetType(Type tpeNamespace, string sType, out Type tpeInterface)
        {
            //Resolving Interface
            try
            {
                if (sType.Contains("`") && sType.Contains("["))
                {
                    //TODO: Need a List or Array Type Resolver
                    tpeInterface = Type.GetType(sType);
                }
                else
                {
                    tpeInterface = this.GetType(tpeNamespace, sType);
                }
                return tpeInterface != null;

            }
            catch (Exception)
            {
                tpeInterface = null;
                return false;
            }
        }

        public MethodInfo ReflectMethod(Type typeNamespace, string sInterface, string sMethod)
        {
            if (typeNamespace == null) { throw new ArgumentNullException("typeNamespace null"); }
            if (sInterface == null) { throw new ArgumentNullException("sInterface null"); }
            if (sMethod == null) { throw new ArgumentNullException("sMethod null"); }

            //Resolving Interface
            Type tpeInterface = null;
            tpeInterface = Type.GetType(typeNamespace.Namespace + "." + sInterface + ", " + typeNamespace.Namespace);

            if (tpeInterface == null) throw new CustomApplicationException("Bad Interface or namespaces");

            //Reflecting Method
            return tpeInterface.GetTypeInfo().GetMethod(sMethod);
        }

        private void ResolveInterfaceAndMethod(Type tpeNamespace, string sInterface, string sMethod, out object oInterfaceResolved, out MethodInfo method)
        {
            if (tpeNamespace == null) { throw new ArgumentNullException("typeNamespace null"); }
            if (sInterface == null) { throw new ArgumentNullException("sInterface null"); }
            if (sMethod == null) { throw new ArgumentNullException("sMethod null"); }

            //Resolving Interface
            Type tpeInterface = null;
            tpeInterface = Type.GetType(tpeNamespace.Namespace + "." + sInterface + ", " + tpeNamespace.Namespace);

            if (tpeInterface == null) throw new CustomApplicationException("Bad Interface or namespaces");
            oInterfaceResolved = container.Resolve(tpeInterface);

            if (oInterfaceResolved == null) throw new CustomApplicationException("Can't Resolve");

            //Reflecting Method
            method = tpeInterface.GetTypeInfo().GetMethod(sMethod);
        }


    }
}
