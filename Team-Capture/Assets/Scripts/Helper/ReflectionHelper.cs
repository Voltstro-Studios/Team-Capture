using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Team_Capture.Helper
{
	/// <summary>
	///     Helper for reflection
	/// </summary>
	public static class ReflectionHelper
	{
		public static IEnumerable<T> GetEnumerableOfType<T>(params object[] constructorArgs) where T : class
		{
			return Assembly.GetAssembly(typeof(T))
				.GetTypes()
				.Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(T)))
				.Select(type => (T) Activator.CreateInstance(type, constructorArgs))
				.ToList();
		}

		/// <summary>
		///     Gets all <see cref="Type" />s that is a sub class of <see cref="T" />
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static IEnumerable<Type> GetInheritedTypes<T>() where T : class
		{
			return Assembly.GetAssembly(typeof(T))
				.GetTypes()
				.Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(T)))
				.ToList();
		}
	}
}