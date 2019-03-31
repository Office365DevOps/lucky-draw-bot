using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace System
{
    public static class ServiceProviderExtensions
    {
        public static object CreateInstance(this IServiceProvider serviceProvider, Type type, Dictionary<Type, object> additionalObjects = null)
        {
            var allConstructorInfos = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            var orderedConstructorInfos = allConstructorInfos.OrderByDescending(ci => ci.GetParameters().Length).ToList();
            if (orderedConstructorInfos.Count < 1)
            {
                throw new Exception($"There is no public constructor in type '{type.FullName}'.");
            }
            var constructorInfo = orderedConstructorInfos.First();

            object[] parameters = BuildParameters(serviceProvider, constructorInfo, additionalObjects);
            return constructorInfo.Invoke(parameters);
        }

        public static T CreateInstance<T>(this IServiceProvider serviceProvider, Dictionary<Type, object> additionalObjects = null)
        {
            return (T)CreateInstance(serviceProvider, typeof(T), additionalObjects);
        }

        public static object InvokeMethod(this IServiceProvider serviceProvider, object instance, string methodName, Dictionary<Type, object> additionalObjects = null)
        {
            var instanceType = instance.GetType();
            var allMethodInfos = instanceType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            var selectedMethods = allMethodInfos.Where(method => method.Name.Equals(methodName)).ToList();
            if (selectedMethods.Count != 1)
            {
                throw new Exception($"There is more than one '{methodName}' method in type '{instanceType.FullName}'.");
            }
            var methodInfo = selectedMethods.First();

            object[] parameters = BuildParameters(serviceProvider, methodInfo, additionalObjects);
            return methodInfo.Invoke(instance, parameters);
        }

        private static object[] BuildParameters(IServiceProvider serviceProvider, MethodBase methodBase, Dictionary<Type, object> additionalObjects)
        {
            var parameterInfos = methodBase.GetParameters();
            var parameters = new object[parameterInfos.Length];
            for (var index = 0; index < parameterInfos.Length; index++)
            {
                var parameterInfo = parameterInfos[index];
                if (parameterInfo.ParameterType == typeof(IServiceProvider))
                {
                    parameters[index] = serviceProvider;
                }
                if ((additionalObjects != null) && additionalObjects.ContainsKey(parameterInfo.ParameterType))
                {
                    parameters[index] = additionalObjects[parameterInfo.ParameterType];
                }
                else
                {
                    try
                    {
                        parameters[index] = serviceProvider.GetRequiredService(parameterInfo.ParameterType);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Failed to get service for parameter '{parameterInfo.ParameterType.FullName} {parameterInfo.Name}' in method '{methodBase.DeclaringType.FullName}.{methodBase.Name}'", ex);
                    }
                }
            }

            return parameters;
        }

        public static TReturn InvokeMethod<TInstance, TReturn>(this IServiceProvider serviceProvider, TInstance instance, string methodName, Dictionary<Type, object> additionalObjects = null)
        {
            return (TReturn)InvokeMethod(serviceProvider, (object)instance, methodName, additionalObjects);
        }

        public static void InvokeMethod<TInstance>(this IServiceProvider serviceProvider, TInstance instance, string methodName)
        {
            InvokeMethod(serviceProvider, (object)instance, methodName);
        }

    }
}
