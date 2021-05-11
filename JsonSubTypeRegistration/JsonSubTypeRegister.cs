using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JsonSubTypes;
using Newtonsoft.Json;

namespace Aptacode.JsonSubTypeRegistration
{
    public class JsonSubTypeRegister
    {
        private readonly Dictionary<Type, HashSet<(Type, Type[])>> _subTypes = new Dictionary<Type, HashSet<(Type, Type[])>>();

        public JsonSubTypeRegister Register<A, B>()
        {
            return Register(typeof(A), typeof(B));
        }

        public JsonSubTypeRegister Register(Type A, Type B)
        {
            var expressionType = B
                .GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == A);

            if (expressionType == default)
                return this;

            var expressionArguments = expressionType.GetGenericArguments().ToArray();

            if (!_subTypes.TryGetValue(expressionType, out var subTypes))
            {
                subTypes = new HashSet<(Type, Type[])>();
                _subTypes[expressionType] = subTypes;
            }
            subTypes.Add((A, expressionArguments));

            return this;
        }

        public JsonSubTypeRegister RegisterAll<T>(params Assembly[] assemblies)
        {
            var type = typeof(T);

            var subTypes = assemblies.SelectMany(a => a.GetTypes().Where(t => t.IsAssignableTo(type)));

            foreach (var subType in subTypes)
            {
                Register(subType, type);
            }

            return this;
        }

        public JsonSerializerSettings AddSubTypes(JsonSerializerSettings settings)
        {
            foreach (var kvp in _subTypes)
            {
                var builder = JsonSubtypesConverterBuilder.Of(kvp.Key, "Exp");
                foreach (var subtype in kvp.Value)
                {
                    builder.RegisterSubtype(subtype.Item1, GetName(subtype));
                }

                settings.Converters.Add(builder
                    .SerializeDiscriminatorProperty(true)
                    .Build());
            }

            return settings;
        }

        public string GetName((Type, Type[]) subType)
        {
            var genericArguments = string.Join("", subType.Item2.Select(t => t.Name));
            return $"{subType.Item1.Name}<{genericArguments}>";
        }
    }
}
