using NetXP.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace NetXP.Reflection.Implementations
{
    public class Reflector : IReflector
    {
        private readonly IContainer container;

        public Reflector(IContainer container)
        {
            this.container = container;
        }

        public object InvokeMethod(Type namespaceType, string @interface, string method, object[] @params)
        {
            ResolveInterfaceAndMethod(namespaceType, @interface, method, out object oInterfaceResolved, out MethodInfo methodInfo);

            //Serialicing Parameter
            ParameterInfo[] aParametersInfo = methodInfo.GetParameters();
            if (aParametersInfo.Count() != (@params?.Length ?? 0)) throw new ReflectorException("Method doesn't have all the parameters.");

            object oReturn = methodInfo.Invoke(oInterfaceResolved, @params);
            return oReturn;
        }

        public object InvokeMethodWithJSONParameters(Type namespaceType, string @interface, string method, string[] @params)
        {
            ResolveInterfaceAndMethod(namespaceType, @interface, method, out object oInterfaceResolved, out MethodInfo methodInfo);

            var aParameterInfo = methodInfo.GetParameters();

            List<object> aParametersToMethod = new List<object>();

            for (int i = 0; i < aParameterInfo.Count(); i++)// var oParameterInfoFE in aParameterInfo)
            {
                ParameterInfo oParameterInfo = aParameterInfo[i];
                var oJsonSerializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(oParameterInfo.ParameterType);
                using (MemoryStream oMS = new MemoryStream())
                {
                    using (StreamWriter oSW = new StreamWriter(oMS))
                    {
                        oSW.Write(@params[i]);
                        oSW.Flush();
                        oMS.Position = 0;
                        var oObject = oJsonSerializer.ReadObject(oMS);
                        aParametersToMethod.Add(oObject);
                    }
                }
            }

            object oReturn = methodInfo.Invoke(oInterfaceResolved, aParametersToMethod.ToArray());
            return oReturn;
        }

        public Type GetType(Type referenceType, string typeToFound)
        {
            var isArray = typeToFound.Contains("[");

            if (isArray)
            {
                typeToFound = typeToFound.Replace("[", "").Replace("]", "");
            }

            //Resolving Interface
            Type typeToTryResolve = null;
            typeToTryResolve = referenceType.GetTypeInfo().Assembly.GetTypes().SingleOrDefault(o => o.Name.Equals(typeToFound));

            if (isArray)
            {
                typeToTryResolve = Type.GetType($"{typeToTryResolve.FullName}[], {typeToTryResolve.Assembly.FullName}");
            }

            if (typeToTryResolve == null)
            {
                throw new ArgumentException($"Type \"{typeToFound}\" not found in assembly with reference type \"{referenceType.Name}\"");
            }
            return typeToTryResolve;
        }

        public bool TryGetType(Type namespaceType, string type, out Type interfaceType)
        {
            //Resolving Interface
            try
            {
                if (type.Contains("`") && type.Contains("["))
                {
                    //TODO: Need a List or Array Type Resolver
                    interfaceType = Type.GetType(type);
                }
                else
                {
                    interfaceType = GetType(namespaceType, type);
                }
                return interfaceType != null;

            }
            catch (Exception)
            {
                interfaceType = null;
                return false;
            }
        }

        public MethodInfo ReflectMethod(Type namespaceType, string @interface, string method)
        {
            if (namespaceType == null) { throw new ArgumentNullException("Namespace Type null"); }
            if (@interface == null) { throw new ArgumentNullException("Interface null"); }
            if (method == null) { throw new ArgumentNullException("Method null"); }

            //Resolving Interface
            Type interfaceType = null;
            interfaceType = Type.GetType(namespaceType.Namespace + "." + @interface + ", " + namespaceType.Assembly.FullName);

            if (interfaceType == null) throw new ReflectorException($"Interface doesn't exist in namespace of type \"{namespaceType.Name}\"");

            //Reflecting Method
            return interfaceType.GetTypeInfo().GetMethod(method);
        }

        public void ResolveInterfaceAndMethod(Type namespaceType, string @interface, string method, out object interfaceResolved, out MethodInfo outMethod)
        {
            if (namespaceType == null) { throw new ArgumentNullException("Namespace Type null"); }
            if (@interface == null) { throw new ArgumentNullException("Interface null"); }
            if (method == null) { throw new ArgumentNullException("Method null"); }

            //Resolving Interface
            Type tpeInterface = null;
            tpeInterface = Type.GetType(namespaceType.Namespace + "." + @interface + ", " + namespaceType.Assembly.FullName);

            if (tpeInterface == null) throw new ReflectorException("Interface doesn't exist.");
            interfaceResolved = container.Resolve(tpeInterface);

            if (interfaceResolved == null) throw new ReflectorException("Can't resolve interface");

            //Reflecting Method
            outMethod = tpeInterface.GetTypeInfo().GetMethod(method);
        }

        public object InvokeMethod(string @interface, string method, object[] @params)
        {
            throw new NotImplementedException();
        }
    }
}
