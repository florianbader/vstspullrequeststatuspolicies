using System.Linq;

namespace System
{
    public static class TypeExtensions
    {
        public static T GetAttribute<T>(this Type type) where T : class
            => type.GetCustomAttributes(typeof(T), true).FirstOrDefault() as T;
    }
}