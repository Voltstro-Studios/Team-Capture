// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Team_Capture.Helper.Extensions
{
    /// <summary>
    ///     Provides extensions for <see cref="GameObject" />s
    /// </summary>
    public static class ObjectsExtension
    {
        private const string DefaultComponentThrowMessage = "Failed to get component of type {0} on object {1}!";
        
        public static void DestroyAllChildren(this Transform trans)
        {
            if (trans.childCount == 0)
                return;

            for (int i = 0; i < trans.childCount; i++) Object.Destroy(trans.GetChild(i));
        }

        /// <summary>
        ///     Gets a component, if it is null then it will immediately throw
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="exceptionMessage"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        public static T GetComponentOrThrow<T>(this Component obj, string exceptionMessage = null)
        {
            T component = obj.GetComponent<T>();
            if (component == null)
            {
                exceptionMessage ??= string.Format(DefaultComponentThrowMessage, typeof(T).FullName, obj.name);
                throw new NullReferenceException(exceptionMessage);
            }
            
            return component;
        }

        /// <summary>
        ///     Gets a component, if it is null then it will immediately throw
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="exceptionMessage"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        public static T GetComponentOrThrow<T>(this GameObject obj, string exceptionMessage = null)
        {
            T component = obj.GetComponent<T>();
            if (component == null)
            {
                exceptionMessage ??= string.Format(DefaultComponentThrowMessage, typeof(T).FullName, obj.name);
                throw new NullReferenceException(exceptionMessage);
            }

            return component;
        }
    }
}