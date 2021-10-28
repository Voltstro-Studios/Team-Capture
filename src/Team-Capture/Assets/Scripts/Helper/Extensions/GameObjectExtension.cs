// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using UnityEngine;

namespace Team_Capture.Helper.Extensions
{
    /// <summary>
    ///     Provides extensions for <see cref="GameObject" />s
    /// </summary>
    public static class GameObjectExtension
    {
        public static void DestroyAllChildren(this Transform trans)
        {
            if (trans.childCount == 0)
                return;

            for (int i = 0; i < trans.childCount; i++) Object.Destroy(trans.GetChild(i));
        }
    }
}