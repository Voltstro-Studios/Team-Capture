// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System.Reflection;

namespace Team_Capture.Helper.Extensions
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