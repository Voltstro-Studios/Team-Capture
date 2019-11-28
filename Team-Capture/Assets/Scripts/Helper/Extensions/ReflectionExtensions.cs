using System.Reflection;

namespace Helper.Extensions
{
    public static class ReflectionExtensions
    {
        public static T GetStaticValue<T>(this FieldInfo field)
        {
            return (T) field.GetValue(null);
        }
    }
}