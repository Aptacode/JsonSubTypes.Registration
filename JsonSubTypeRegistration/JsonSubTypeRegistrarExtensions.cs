using Newtonsoft.Json;

namespace Aptacode.JsonSubTypeRegistration
{
    public static class JsonSubTypeRegistrarExtensions
    {
        public static JsonSerializerSettings Add(this JsonSerializerSettings settings, JsonSubTypeRegister register)
        {
            return register.AddSubTypes(settings);
        }
    }
}