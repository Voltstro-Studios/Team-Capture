using System.Reflection;

namespace Helper.Extensions
{
    public static class ReflectionExtensions
    {
        public static T GetStaticValue<T>(this FieldInfo field)
        {
            return (T) field.GetValue(null);
        }

        public static T GetValue<T>(this FieldInfo field, object instance)
        {
            return (T) field.GetValue(instance);
        }
        
        public static T GetStaticValue<T>(this PropertyInfo property)
        {
            return (T) property.GetValue(null);
        }

        public static T GetValue<T>(this PropertyInfo property, object instance)
        {
            return (T) property.GetValue(instance);
        }
    }
}