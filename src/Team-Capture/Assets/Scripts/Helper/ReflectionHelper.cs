// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace Team_Capture.Helper
{
    /// <summary>
    ///     Helper for reflection
    /// </summary>
    public static class ReflectionHelper
    {
        /// <summary>
        ///     Gets all <see cref="Type" />s that is a sub class of <see cref="T" />
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IReadOnlyList<Type> GetInheritedTypes<T>() where T : class
        {
            return Assembly.GetAssembly(typeof(T))
                .GetTypes()
                .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(T)))
                .ToList();
        }
    }
}